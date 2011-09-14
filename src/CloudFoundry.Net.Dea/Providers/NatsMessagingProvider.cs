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
    using Interfaces;
    using NLog;
    using Types;

    public class NatsMessagingProvider : IMessagingProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan DEFAULT_INTERVAL = TimeSpan.FromMilliseconds(50);

        private const string LogSentFormat = "NATS Msg Sent: {0}";
        private const string LogReceivedFormat = "NATS Msg Recv: {0}";
        private const string CRLF = "\r\n";

        private string host;
        private ushort port;

        private TcpClient client;
        private NetworkStream stream;
        private Dictionary<string, Dictionary<int, Action<string, string>>> subscriptions;
        private bool disposing = false;
        private int sequence = 1;

        private readonly Queue<string> messageQueue = new Queue<string>();

        private Task queueTask;
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

        public string UniqueIdentifier { get; private set; }

        public int Sequence { get { return sequence; } }

        public void Publish(string argSubject, Message argMessage)
        {
            Publish(argSubject, argMessage.ToJson());
        }

        public void Publish(string subject, string message)
        {
            Logger.Debug("NATS Publishing subject: {0},{1}", subject, message);  
            var formattedMessage = string.Format(Constants.NatsCommandFormats.Publish, subject, Encoding.ASCII.GetBytes(message).Length, message);
            Logger.Trace(LogSentFormat, formattedMessage);
            Write(formattedMessage);
        }        

        public void Subscribe(string subject, Action<string, string> replyCallback)
        {
            Interlocked.Increment(ref sequence);

            Logger.Debug("NATS Subscribing to subject: {0}, sequence {1}", subject, Sequence);            
            var formattedMessage = string.Format(Constants.NatsCommandFormats.Subscribe, subject, Sequence);
            Logger.Trace(LogSentFormat,formattedMessage);
            Write(formattedMessage);
            
            lock (subscriptions)
            { 
                if (!subscriptions.ContainsKey(subject))
                    subscriptions.Add(subject, new Dictionary<int,Action<string,string>>());
                subscriptions[subject].Add(Sequence, replyCallback);
            }
        }

        private void messageProcessor()
        {
            Task currentTask = null;

            while (false == disposing)
            {
                Thread.Sleep(DEFAULT_INTERVAL);

                if (false == messageQueue.IsNullOrEmpty() && (null == currentTask || currentTask.IsCompleted))
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

                        Action<string, string> callback = subjectCollection[receivedMessage.SubscriptionID];
                        currentTask = Task.Factory.StartNew(() => callback(receivedMessage.RawMessage, receivedMessage.InboxID));
                    }
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough] // NB: this allows "break on exceptions" to be enabled in VS without having that IOException break all the time
        private void poll()
        {
            string incomingData = String.Empty;
            byte[] buffer = new byte[1024];

            while (false == disposing)
            {
                Thread.Sleep(DEFAULT_INTERVAL);

                if (false == stream.CanRead)
                {
                    throw new ApplicationException("Can't read from network stream!");
                }

                int receivedDataLength = 0;
                try
                {
                    receivedDataLength = stream.Read(buffer, 0, buffer.Length); // NB: blocking
                }
                catch (IOException)
                {
                    // NB: http://msdn.microsoft.com/en-us/library/bk6w7hs8.aspx
                }
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
            }
        }

        public void Start()
        {
            pollTask = Task.Factory.StartNew(poll);
            queueTask = Task.Factory.StartNew(messageProcessor);
        }

        public void Connect()
        {            
            client = new TcpClient(host, port);
            Logger.Debug("NATS Connected on Host: {0}, Port: {1}", host, port);
            stream = client.GetStream();
            if (stream.CanTimeout)
            {
                stream.ReadTimeout = DEFAULT_INTERVAL.Milliseconds;
            }
        }        

        public void Dispose()
        {
            try
            {
                var tasks = new[] { pollTask, queueTask };
                disposing = true;
                Logger.Debug("NATS Waiting for polling to cease.");
                Task.WaitAll(tasks);
                Logger.Debug("NATS Disconnected.");
                stream.Close();
                stream.Dispose();
                client.Close();
            }
            catch { };
        }        

        private void Write(string message)
        {
            Byte[] data = ASCIIEncoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }             
    }

    public class ReceivedMessage
    {
        public string RawMessage { get; private set; }
        public string Subject { get; private set; }
        public string InboxID { get; private set; }
        public int SubscriptionID { get; private set; }
        public int Size { get; private set; }

        public ReceivedMessage(string message)
        {
            Regex stdMsg = new Regex(@"MSG\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)[\r\n]+([^\r\n]+)", RegexOptions.None);
            Regex inboxMsg = new Regex(@"MSG\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)\s+([^\s]+)[\r\n]+([^\r\n]+)", RegexOptions.None);
            
            var matches = stdMsg.Matches(message);
            if (matches.Count > 0)
            {
                var groups = matches[0].Groups;
                Subject = groups[1].Value;
                SubscriptionID = Convert.ToInt32(groups[2].Value);
                Size = Convert.ToInt32(groups[3].Value);
                RawMessage = groups[4].Value;
            }
            else
            {
                var inboxMatches = inboxMsg.Matches(message);
                if (inboxMatches.Count > 0)
                {
                    var groups = inboxMatches[0].Groups;
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