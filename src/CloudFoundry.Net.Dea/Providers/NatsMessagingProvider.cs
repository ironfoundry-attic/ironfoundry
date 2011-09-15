namespace CloudFoundry.Net.Dea.Providers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using NLog;
    using Properties;
    using Types;

    public class NatsMessagingProvider : IMessagingProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

#if DEBUG
        private static readonly TimeSpan DEFAULT_INTERVAL = TimeSpan.FromMilliseconds(250);
        private static readonly ushort CONNECTION_ATTEMPT_RETRIES = 1;
        private static readonly TimeSpan CONNECTION_ATTEMPT_INTERVAL = TimeSpan.FromSeconds(1);
#else
        private static readonly TimeSpan DEFAULT_INTERVAL = TimeSpan.FromMilliseconds(250);
        private static readonly ushort CONNECTION_ATTEMPT_RETRIES = 5;
        private static readonly TimeSpan CONNECTION_ATTEMPT_INTERVAL = TimeSpan.FromSeconds(10);
#endif

        private const string LogSentFormat = "NATS Msg Sent: {0}";
        private const string LogReceivedFormat = "NATS Msg Recv: {0}";
        private const string CRLF = "\r\n";

        private string host;
        private ushort port;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private Dictionary<string, Dictionary<int, Action<string, string>>> subscriptions;
        private int sequence = 1;

        private bool shutting_down = false;
        private bool attempting_reconnect = false;
        private bool error_occurred = false;

        private readonly Queue<string> messageQueue = new Queue<string>();

        private Task messageProcessorTask;
        private Task pollTask;

        public NatsMessagingProvider(string argHost, ushort argPort)
        {
            host = argHost;
            port = argPort;
            sequence = 1;
            UniqueIdentifier = Guid.NewGuid().ToString("N");   
            Logger.Debug("NATS Messaging Provider Initialized. Identifier: {0}, Server Host: {1}, Server Port: {2}.", UniqueIdentifier, argHost, port);
            subscriptions = new Dictionary<string, Dictionary<int, Action<string, string>>>();
        }

        public NatsMessagingStatus Status { get; private set; }

        public string StatusMessage { get; private set; }

        public string UniqueIdentifier { get; private set; }

        public int Sequence { get { return sequence; } }

        public void Publish(string argSubject, Message argMessage)
        {
            Publish(argSubject, argMessage.ToJson());
        }

        public void Publish(string subject, string message)
        {
            if (NatsMessagingStatus.RUNNING != Status)
                return;

            Logger.Debug("NATS Publishing subject: {0},{1}", subject, message);  

            string formattedMessage = String.Format(Constants.NatsCommandFormats.Publish, subject, Encoding.ASCII.GetBytes(message).Length, message);

            Logger.Trace(LogSentFormat, formattedMessage);

            write(formattedMessage);
        }        

        public void Subscribe(string subject, Action<string, string> replyCallback)
        {
            if (NatsMessagingStatus.RUNNING != Status)
                return;

            Interlocked.Increment(ref sequence);

            Logger.Debug("NATS Subscribing to subject: {0}, sequence {1}", subject, Sequence);            
            
            string formattedMessage = String.Format(Constants.NatsCommandFormats.Subscribe, subject, Sequence);

            Logger.Trace(LogSentFormat,formattedMessage);

            write(formattedMessage);
            
            lock (subscriptions)
            { 
                if (!subscriptions.ContainsKey(subject))
                    subscriptions.Add(subject, new Dictionary<int,Action<string,string>>());
                subscriptions[subject].Add(Sequence, replyCallback);
            }
        }

        public bool Connect()
        {
            if (NatsMessagingStatus.RUNNING != Status)
                return false;

            bool rv = false;

            for (ushort i = 0;
                NatsMessagingStatus.RUNNING == Status && i < CONNECTION_ATTEMPT_RETRIES;
                ++i)
            {
                try
                {
                    tcpClient = new TcpClient(host, port);
                    networkStream = tcpClient.GetStream();
                    if (networkStream.CanTimeout)
                    {
                        networkStream.ReadTimeout = (int)DEFAULT_INTERVAL.TotalMilliseconds; // NB: must use TotalMilliseconds
                    }
                    rv = true;
                }
                catch (SocketException ex)
                {
                    if (SocketErrorCode.ConnectionRefused == (SocketErrorCode)ex.ErrorCode)
                    {
                        Thread.Sleep(CONNECTION_ATTEMPT_INTERVAL);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (rv)
            {
                Logger.Debug("NATS Connected on Host: {0}, Port: {1}", host, port);
            }
            else
            {
                Logger.Fatal("NATS could not connect to Host: {0}, Port: {1}", host, port);
            }

            return rv;
        }        

        public void Start()
        {
            pollTask = Task.Factory.StartNew(poll);
            messageProcessorTask = Task.Factory.StartNew(messageProcessor);
            Status = NatsMessagingStatus.RUNNING;
        }

        public void Stop()
        {
            if (shutting_down)
            {
                throw new InvalidOperationException(Resources.NatsMessagingProvider_AttemptingStopTwice_Message);
            }

            Status = NatsMessagingStatus.STOPPING;

            try
            {
                var tasks = new[] { pollTask, messageProcessorTask };
                shutting_down = true;
                Logger.Debug("NATS Waiting for polling to cease.");
                Task.WaitAll(tasks);
                closeNetworking();
                Logger.Debug("NATS Disconnected.");
            }
            catch { };

            if (error_occurred)
            {
                Status = NatsMessagingStatus.ERROR;
            }
            else
            {
                Status = NatsMessagingStatus.STOPPED;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void onFatalError(string argMessage = "")
        {
            error_occurred = true;
            Status = NatsMessagingStatus.ERROR;
            StatusMessage = argMessage;
        }

        private void messageProcessor()
        {
            while (NatsMessagingStatus.RUNNING == Status)
            {
                Thread.Sleep(DEFAULT_INTERVAL);

                if (false == messageQueue.IsNullOrEmpty())
                {
                    string message = messageQueue.Dequeue();
                    if (String.IsNullOrEmpty(message))
                    {
                        // NB: SHOULD NEVER HAPPEN
                        continue;
                    }

                    Logger.Trace(LogReceivedFormat, message);

                    if (Constants.NatsCommands.Ok == message)
                    {
                        Logger.Trace("NATS Message Acknowledged: {0}", message);
                    }
                    else if (message.StartsWith(Constants.NatsCommands.Information))
                    {
                        Logger.Trace("NATS Info Message {0}", message);
                    }
                    else if (message.StartsWith(Constants.NatsCommands.Message))
                    {
                        // We can't guarantee that the continuation will be on the queue
                        while (messageQueue.IsNullOrEmpty())
                        {
                            Thread.Sleep(DEFAULT_INTERVAL);
                        }

                        string messageContinuation = messageQueue.Dequeue();

                        var receivedMessage = new ReceivedMessage(message + CRLF + messageContinuation);

                        if (false == subscriptions.ContainsKey(receivedMessage.Subject))
                        {
                            Logger.Debug("NATS Message Subject: {0} not found to be subscribed. Ignoring received message {1},{2}.", receivedMessage.Subject, receivedMessage.SubscriptionID, receivedMessage.RawMessage);
                            continue;
                        }

                        var subjectCollection = subscriptions[receivedMessage.Subject];
                        if (false == subjectCollection.ContainsKey(receivedMessage.SubscriptionID))
                        {
                            Logger.Debug("NATS Message Subscription ID: {0} not found to be subscribed for subject {1}. Ignoring received message {2}.", receivedMessage.SubscriptionID, receivedMessage.Subject, receivedMessage.RawMessage);
                            continue;
                        }

                        if (NatsMessagingStatus.RUNNING == Status)
                        {
                            Action<string, string> callback = subjectCollection[receivedMessage.SubscriptionID];
                            callback(receivedMessage.RawMessage, receivedMessage.InboxID);
                        }
                    }
                }
            }
        }

        // NB: this allows "break on exceptions" to be enabled in VS without having that IOException break all the time
        // [System.Diagnostics.DebuggerStepThrough]
        private void poll()
        {
            string incomingData = String.Empty;
            byte[] buffer = new byte[1024];

            while (NatsMessagingStatus.RUNNING == Status)
            {
                Thread.Sleep(DEFAULT_INTERVAL);

                if (NatsMessagingStatus.RUNNING != Status)
                    return;

                if (false == networkStream.CanRead)
                {
                    onFatalError("Can't read from network stream!");
                }

                if (NatsMessagingStatus.RUNNING != Status)
                    return;

                if (false == StillConnected)
                {
                    reconnect();
                }

                if (NatsMessagingStatus.RUNNING != Status)
                    return;

                int receivedDataLength = 0;
                try
                {
                    receivedDataLength = networkStream.Read(buffer, 0, buffer.Length);
                }
                catch (IOException ex)
                {
                    bool shouldRethrow = dealWithException(ex);
                    if (shouldRethrow)
                    {
                        throw;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (NatsMessagingStatus.RUNNING != Status)
                    return;

                if (receivedDataLength > 0)
                {
                    incomingData += Encoding.ASCII.GetString(buffer, 0, receivedDataLength);

                    if (incomingData.Contains(CRLF)) // CRLF == at least one message
                    {
                        // Read a complete message
                        string[] messages = incomingData.Split(new[] { CRLF }, StringSplitOptions.RemoveEmptyEntries);

                        // TODO lock queue?
                        if (false == incomingData.EndsWith(CRLF))
                        {
                            incomingData = messages.Last(); // Last bit of data is incomplete, preserve
                            for (uint i = 0; i < messages.Length; ++i) // NB: will leave off the last partial message
                            {
                                if (false == String.IsNullOrWhiteSpace(messages[i]))
                                {
                                    messageQueue.Enqueue(messages[i]);
                                }
                            }
                        }
                        else
                        {
                            incomingData = String.Empty;
                            foreach (string msg in messages.Where(msg => false == String.IsNullOrWhiteSpace(msg)))
                            {
                                messageQueue.Enqueue(msg);
                            }
                        }
                    }
                }

                if (NatsMessagingStatus.RUNNING != Status)
                    return;
            }
        }

        private void reconnect()
        {
            if (NatsMessagingStatus.RUNNING != Status)
            {
                return;
            }

            if (attempting_reconnect)
            {
                throw new InvalidOperationException(Resources.NatsMessagingProvider_AttemptingReconnectTwice_Message);
            }

            try
            {
                attempting_reconnect = true;
                closeNetworking();
                bool success = Connect();
                if (false == success && NatsMessagingStatus.RUNNING == Status)
                {
                    onFatalError(Resources.NatsMessagingProvider_CouldNotReconnect_Message);
                }
            }
            finally
            {
                attempting_reconnect = false;
            }
        }

        private void closeNetworking()
        {
            try
            {
                networkStream.Close();
                networkStream.Dispose();
                tcpClient.Close();
            }
            catch { }
        }

        private void write(string message)
        {
            if (NatsMessagingStatus.RUNNING != Status)
                return;

        Retry:
            try
            {
                Byte[] data = ASCIIEncoding.ASCII.GetBytes(message);
                networkStream.Write(data, 0, data.Length);
                /*
                 * NB: Flush () not necessary
                 * http://msdn.microsoft.com/en-us/library/system.net.sockets.networkstream.flush.aspx
                 */
            }
            catch (IOException ex)
            {
                bool shouldRethrow = dealWithException(ex);
                if (shouldRethrow)
                {
                    throw;
                }
                else
                {
                    // Give it another shot
                    Thread.Sleep(DEFAULT_INTERVAL);
                    goto Retry;
                }
            }
        }

        private bool dealWithException(IOException ex)
        {
            bool rethrow = false;

            SocketException inner = ex.InnerException as SocketException;
            if (null != inner)
            {
                SocketErrorCode errorCode = (SocketErrorCode)inner.ErrorCode;
                switch (errorCode)
                {
                    case SocketErrorCode.ConnectionTimedOut :
                        // Ignore!
                        // NB: http://msdn.microsoft.com/en-us/library/bk6w7hs8.aspx
                        break;
                    case SocketErrorCode.ConnectionAborted :
                        reconnect();
                        break;
                    default :
                        rethrow = true;
                        break;
                }
            }

            return rethrow;
        }

        private bool StillConnected
        {
            get { return tcpClient.Client.IsConnected(); }
        }

        private class ReceivedMessage
        {
            private static readonly Regex stdMsg = new Regex(@"MSG\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)[\r\n]+([^\r\n]+)", RegexOptions.Compiled);
            private static readonly Regex inboxMsg = new Regex(@"MSG\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)[\r\n]+([^\r\n]+)", RegexOptions.Compiled);

            public string RawMessage { get; private set; }
            public string Subject { get; private set; }
            public string InboxID { get; private set; }
            public int SubscriptionID { get; private set; }
            public int Size { get; private set; }

            public ReceivedMessage(string message)
            {
                
                MatchCollection matches = stdMsg.Matches(message);
                if (matches.Count > 0)
                {
                    GroupCollection groups = matches[0].Groups;
                    Subject = groups[1].Value;
                    SubscriptionID = Convert.ToInt32(groups[2].Value);
                    Size = Convert.ToInt32(groups[3].Value);
                    RawMessage = groups[4].Value;
                }
                else
                {
                    MatchCollection inboxMatches = inboxMsg.Matches(message);
                    if (inboxMatches.Count > 0)
                    {
                        GroupCollection groups = inboxMatches[0].Groups;
                        Subject = groups[1].Value;                    
                        SubscriptionID = Convert.ToInt32(groups[2].Value);
                        InboxID = groups[3].Value;
                        Size = Convert.ToInt32(groups[4].Value);
                        RawMessage = groups[5].Value;
                    }
                }            
            }
        }
    }
}