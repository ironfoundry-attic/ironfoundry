namespace IronFoundry.Dea.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Types;

    public class NatsMessagingProvider : IMessagingProvider
    {
        private static readonly ushort CONNECTION_ATTEMPT_RETRIES = 10;

        private static readonly TimeSpan DEFAULT_INTERVAL = TimeSpan.FromMilliseconds(250);
        private static readonly TimeSpan CONNECTION_ATTEMPT_INTERVAL = TimeSpan.FromSeconds(10);

        private static class NatsCommandFormats
        {
            /// <summary>
            /// Format for sending publish messages. First parameter is the subject,
            /// second parameter is the actual byte length of the message (you should
            /// retrieve this by converting the string to an ascii message than retrieving
            /// the byte array length that's produced),
            /// third parameter is the actual message (should be in JSON format).
            /// </summary>
            public static readonly string Publish = NatsCommand.Publish.Command + " {0}  {1}\r\n{2}\r\n";

            /// <summary>
            /// Format for sending subscribe message. First parameter is the subject,
            /// second parameter is a sequential integer identifier. For every subscribe message
            /// a running tally of unique integers is used in order to reply back to the
            /// subscribed message.
            /// </summary>
            public static readonly string Subscribe = NatsCommand.Subscribe.Command + " {0}  {1}\r\n";
        }

        private const string LogSentFormat = "NATS Msg Sent: {0}";
        private const string LogReceivedFormat = "NATS Msg Recv: {0}";
        private const string CRLF = "\r\n";

        private readonly string host;
        private readonly ushort port;
        private readonly IDictionary<string, IDictionary<int, Action<string, string>>> subscriptions
            = new Dictionary<string, IDictionary<int, Action<string, string>>>();
        private readonly Guid uniqueIdentifier;

        private int sequence = 1;
        private TcpClient tcpClient;
        private bool shutting_down = false;
        private bool error_occurred = false;

        private readonly Queue<string> messageQueue = new Queue<string>();

        private Task messageProcessorTask;
        private Task pollTask;

        private readonly ILog log;

        public NatsMessagingProvider(ILog log, IConfig config)
        {
            this.log  = log;
            this.host = config.NatsHost;
            this.port = config.NatsPort;

            uniqueIdentifier = Guid.NewGuid();
            sequence = 1;
            log.Debug("NATS Messaging Provider Initialized. Identifier: {0:N}, Server Host: {1}, Server Port: {2}.", UniqueIdentifier, host, port);
        }

        public NatsMessagingStatus Status { get; private set; }

        public string StatusMessage { get; private set; }

        public Guid UniqueIdentifier { get { return uniqueIdentifier; } }

        public int Sequence { get { return sequence; } }

        public void Publish(string argSubject, Message argMessage)
        {
            Hello helloMessage = argMessage as Hello;
            if (null != helloMessage)
            {
                DoPublish(argSubject, helloMessage);
                return;
            }

            FindDropletResponse findDropletResponse = argMessage as FindDropletResponse;
            if (null != findDropletResponse)
            {
                DoPublish(argSubject, findDropletResponse);
                return;
            }

            throw new InvalidOperationException(
                String.Format("Invalid attempt to publish message of type '{0}' with subject '{1}'", argMessage.GetType().Name, argSubject));
        }

        public void Publish(Message argMessage)
        {
            DoPublish(argMessage.PublishSubject, argMessage);
        }

        public void Publish(NatsCommand argCommand, Message argMessage)
        {
            DoPublish(argCommand.Command, argMessage);
        }

        public void Subscribe(NatsSubscription argSubscription, Action<string, string> argCallback)
        {
            if (NatsMessagingStatus.RUNNING != Status)
                return;

            Interlocked.Increment(ref sequence);

            log.Debug("NATS Subscribing to subject: {0}, sequence {1}", argSubscription, Sequence);

            string formattedMessage = String.Format(NatsCommandFormats.Subscribe, argSubscription, Sequence);

            log.Trace(LogSentFormat, formattedMessage);

            Write(formattedMessage);

            lock (subscriptions)
            {
                if (false == subscriptions.ContainsKey(argSubscription.Subscription))
                {
                    subscriptions.Add(argSubscription.Subscription, new Dictionary<int, Action<string, string>>());
                }
                subscriptions[argSubscription.Subscription].Add(Sequence, argCallback);
            }
        }

        public bool Connect()
        {
            if (NatsMessagingStatus.RUNNING != Status)
            {
                return false;
            }

            bool rv = false;

            for (ushort i = 0; NatsMessagingStatus.RUNNING == Status && i < CONNECTION_ATTEMPT_RETRIES; ++i)
            {
                try
                {
                    tcpClient = new TcpClient(host, port);
                    NetworkStream networkStream = tcpClient.GetStream();
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
                log.Debug("NATS Connected on Host: {0}, Port: {1}", host, port);
            }
            else
            {
                log.Fatal("NATS could not connect to Host: {0}, Port: {1}", host, port);
            }

            return rv;
        }

        public void Start()
        {
            pollTask = Task.Factory.StartNew(Poll);
            messageProcessorTask = Task.Factory.StartNew(MessageProcessor);
            Status = NatsMessagingStatus.RUNNING;
        }

        public void Stop()
        {
            if (shutting_down)
            {
                throw new InvalidOperationException(IronFoundry.Dea.Properties.Resources.NatsMessagingProvider_AttemptingStopTwice_Message);
            }

            Status = NatsMessagingStatus.STOPPING;

            try
            {
                var tasks = new[] { pollTask, messageProcessorTask };
                shutting_down = true;
                log.Debug("NATS Waiting for polling to cease.");
                Task.WaitAll(tasks);
                CloseNetworking();
                log.Debug("NATS Disconnected.");
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

        private void DoPublish(string argSubject, Message argMessage)
        {
            if (Message.RECEIVE_ONLY == argSubject)
            {
                throw new InvalidOperationException("Attempt to publish receive-only message!");
            }

            if (NatsMessagingStatus.RUNNING != Status)
            {
                return;
            }

            log.Debug("NATS Publishing subject: {0},{1}", argSubject, argMessage);

            string messageJson = argMessage.ToJson();

            string formattedMessage = String.Format(CultureInfo.InvariantCulture,
                NatsCommandFormats.Publish, argSubject, Encoding.ASCII.GetBytes(messageJson).Length, messageJson);

            log.Trace(LogSentFormat, formattedMessage);

            Write(formattedMessage);
        }

        private void OnFatalError(string argMessage = "")
        {
            error_occurred = true;
            Status = NatsMessagingStatus.ERROR;
            StatusMessage = argMessage;
        }

        private void MessageProcessor()
        {
            while (NatsMessagingStatus.RUNNING == Status)
            {
                Thread.Sleep(DEFAULT_INTERVAL);

                string message = null;
                lock (messageQueue)
                {
                    if (false == messageQueue.IsNullOrEmpty())
                    {
                        message = messageQueue.Dequeue();
                    }
                }

                if (false == String.IsNullOrWhiteSpace(message))
                {
                    log.Trace(LogReceivedFormat, message);

                    if (NatsCommand.Ok.Command == message)
                    {
                        log.Trace("NATS Message Acknowledged: {0}", message);
                    }
                    else if (message.StartsWith(NatsCommand.Information.Command))
                    {
                        log.Trace("NATS Info Message {0}", message);
                    }
                    else if (message.StartsWith(NatsCommand.Message.Command))
                    {
                        string messageContinuation = null;
                        while (true)
                        {
                            lock (messageQueue)
                            {
                                if (false == messageQueue.IsNullOrEmpty())
                                {
                                    messageContinuation = messageQueue.Dequeue();
                                    break;
                                }
                            }

                            if (String.IsNullOrWhiteSpace(messageContinuation))
                            {
                                Thread.Sleep(DEFAULT_INTERVAL);
                            }
                        }

                        log.Trace(LogReceivedFormat, messageContinuation);

                        var receivedMessage = new ReceivedMessage(message + CRLF + messageContinuation);

                        if (false == subscriptions.ContainsKey(receivedMessage.Subject))
                        {
                            log.Debug("NATS Message Subject: {0} not found to be subscribed. Ignoring received message {1},{2}.", receivedMessage.Subject, receivedMessage.SubscriptionID, receivedMessage.RawMessage);
                            continue;
                        }

                        var subjectCollection = subscriptions[receivedMessage.Subject];
                        if (false == subjectCollection.ContainsKey(receivedMessage.SubscriptionID))
                        {
                            log.Debug("NATS Message Subscription ID: {0} not found to be subscribed for subject {1}. Ignoring received message {2}.", receivedMessage.SubscriptionID, receivedMessage.Subject, receivedMessage.RawMessage);
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
        [System.Diagnostics.DebuggerStepThrough]
        private void Poll()
        {
            string incomingData = String.Empty;
            byte[] buffer = new byte[1024];

            while (NatsMessagingStatus.RUNNING == Status)
            {
                Thread.Sleep(DEFAULT_INTERVAL);

                if (NatsMessagingStatus.RUNNING != Status)
                {
                    return;
                }

                if (tcpClient.Connected && false == tcpClient.GetStream().CanRead)
                {
                    OnFatalError(IronFoundry.Dea.Properties.Resources.NatsMessagingProvider_CantReadFromStream_Message);
                }

                if (NatsMessagingStatus.RUNNING != Status)
                {
                    return;
                }

                if (false == tcpClient.Connected)
                {
                    OnFatalError(IronFoundry.Dea.Properties.Resources.NatsMessagingProvider_Disconnected_Message);
                }

                if (NatsMessagingStatus.RUNNING != Status)
                {
                    return;
                }

                int receivedDataLength = 0;
                try
                {
                    receivedDataLength = tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                }
                catch (IOException ex)
                {
                    bool shouldRethrow = HandleException(ex);
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
                {
                    return;
                }

                if (receivedDataLength > 0)
                {
                    incomingData += Encoding.ASCII.GetString(buffer, 0, receivedDataLength);

                    if (incomingData.Contains(CRLF)) // CRLF == at least one message
                    {
                        // Read a complete message
                        string[] messages = incomingData.Split(new[] { CRLF }, StringSplitOptions.RemoveEmptyEntries);
                        lock (messageQueue)
                        {
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
                }

                if (NatsMessagingStatus.RUNNING != Status)
                {
                    return;
                }
            }
        }

        private void CloseNetworking()
        {
            tcpClient.GetStream().Close();
            tcpClient.Close();
        }

        private void Write(string message)
        {
            if (NatsMessagingStatus.RUNNING != Status)
            {
                return;
            }

            while (NatsMessagingStatus.RUNNING == Status)
            {
                try
                {
                    Byte[] data = ASCIIEncoding.ASCII.GetBytes(message);
                    tcpClient.GetStream().Write(data, 0, data.Length);
                    /*
                     * NB: Flush () not necessary
                     * http://msdn.microsoft.com/en-us/library/system.net.sockets.networkstream.flush.aspx
                     */
                    return;
                }
                catch (IOException ex)
                {
                    log.Error(ex, Resources.NatsMessagingProvider_ExceptionSendingMessage_Fmt, message);
                    bool shouldRethrow = HandleException(ex);
                    if (shouldRethrow)
                    {
                        throw;
                    }
                }
            }
        }

        private bool HandleException(IOException ex)
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
                        OnFatalError(IronFoundry.Dea.Properties.Resources.NatsMessagingProvider_Disconnected_Message);
                        break;
                    default :
                        rethrow = true;
                        break;
                }
            }

            return rethrow;
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