namespace IronFoundry.Dea.Providers
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
    using System.Timers;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Types;
    using Timer = System.Timers.Timer;

    public class NatsMessagingProvider : IMessagingProvider
    {
        private static readonly ushort ConnectionAttemptRetries = 60;

        private readonly TimeSpan Seconds_5 = TimeSpan.FromSeconds(5);
        private readonly TimeSpan Seconds_10 = TimeSpan.FromSeconds(10);

        private const string CRLF = "\r\n";

        private readonly string host;
        private readonly ushort port;

        private readonly IDictionary<string, NatsSubscription> subscriptionsByName =
            new Dictionary<string, NatsSubscription>();
        private readonly IDictionary<NatsSubscription, IDictionary<int, Action<string, string>>> subscriptions
            = new Dictionary<NatsSubscription, IDictionary<int, Action<string, string>>>();

        private readonly Guid uniqueIdentifier;

        private bool shuttingDown = false;

        private TcpClient tcpClient;

        private readonly Queue<string> messageQueue = new Queue<string>();
        private readonly AutoResetEvent messageQueuedEvent = new AutoResetEvent(false);

        private readonly Queue<string> messagesPendingWrite = new Queue<string>();

        private Task messageProcessorTask;
        private Task pollTask;

        private readonly ILog log;

        public NatsMessagingProvider(ILog log, IConfig config)
        {
            this.log  = log;
            this.host = config.NatsHost;
            this.port = config.NatsPort;

            uniqueIdentifier = Guid.NewGuid();
            log.Debug(Resources.NatsMessagingProvider_Initialized_Fmt, UniqueIdentifier, host, port);

            Status = NatsMessagingStatus.RUNNING;
        }

        public NatsMessagingStatus Status { get; private set; }

        public Guid UniqueIdentifier { get { return uniqueIdentifier; } }

        public void Publish(string subject, Message message)
        {
            if (message.CanPublishWithSubject(subject))
            {
                DoPublish(subject, message);
            }
            else
            {
                throw new InvalidOperationException(String.Format(Resources.NatsMessagingProvider_InvalidPublishAttempt_Fmt, message.GetType().Name, subject));
            }
        }

        public void Publish(string subject, Message message, uint delay)
        {
            if (message.CanPublishWithSubject(subject))
            {
                DoPublish(subject, message, delay);
            }
            else
            {
                throw new InvalidOperationException(String.Format(Resources.NatsMessagingProvider_InvalidPublishAttempt_Fmt, message.GetType().Name, subject));
            }
        }

        public void Publish(Message message)
        {
            DoPublish(message.PublishSubject, message);
        }

        public void Publish(NatsCommand command, Message message)
        {
            DoPublish(command.Command, message);
        }

        public void Subscribe(NatsSubscription subscription, Action<string, string> callback)
        {
            if (NatsMessagingStatus.RUNNING != Status)
            {
                return;
            }

            SendSubscription(subscription);

            lock (subscriptions)
            {
                if (false == subscriptions.ContainsKey(subscription))
                {
                    subscriptions.Add(subscription, new Dictionary<int, Action<string, string>>());
                    subscriptionsByName.Add(subscription.ToString(), subscription);
                }
                subscriptions[subscription].Add(subscription.Sequence, callback);
            }
        }

        public bool Start()
        {
            bool rv = false;
            Status = NatsMessagingStatus.RUNNING;
            if (Connect())
            {
                pollTask = Task.Factory.StartNew(Poll);
                messageProcessorTask = Task.Factory.StartNew(MessageProcessor);
                rv = true;
            }

            return rv;
        }

        public void Stop()
        {
            if (shuttingDown)
            {
                throw new InvalidOperationException(Resources.NatsMessagingProvider_AttemptingStopTwice_Message);
            }

            Status = NatsMessagingStatus.STOPPING;
            
            shuttingDown = true;

            CloseNetworking();

            var tasks = new List<Task>();
            if (null != pollTask)
            {
                tasks.Add(pollTask);
            }
            if (null != messageProcessorTask)
            {
                tasks.Add(messageProcessorTask);
            }

            log.Debug(Resources.NatsMessagingProvider_WaitingForTasks_Message);
            if (false == tasks.IsNullOrEmpty())
            {
                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch (AggregateException ex)
                {
                    foreach (Exception inner in ex.Flatten().InnerExceptions)
                    {
                        log.Error(ex);
                    }
                }
            }
            log.Debug(Resources.NatsMessagingProvider_Disconnected_Message);
            Status = NatsMessagingStatus.STOPPED;
        }

        public void Dispose()
        {
            Stop();
            messageQueuedEvent.Dispose();
        }

        private bool Reconnect()
        {
            log.Info(Resources.NatsMessagingProvider_AttemptingReconnect_Message);

            CloseNetworking();

            bool connected = Connect();

            if (connected)
            {
                lock (subscriptions)
                {
                    foreach (NatsSubscription subscription in subscriptions.Keys)
                    {
                        SendSubscription(subscription);
                    }
                }
                foreach (string message in messagesPendingWrite)
                {
                    Write(message);
                }
                messagesPendingWrite.Clear();
            }
            return connected;
        }

        private bool Connect()
        {
            if (NatsMessagingStatus.RUNNING != Status)
            {
                return false;
            }

            bool rv = false;

            for (ushort i = 1; NatsMessagingStatus.RUNNING == Status && i <= ConnectionAttemptRetries; ++i)
            {
                try
                {
                    tcpClient = new TcpClient(host, port)
                    {
                        LingerState = new LingerOption(true, 0),
                        NoDelay = true
                    };
                    rv = true;
                }
                catch (SocketException ex)
                {
                    if (SocketError.ConnectionRefused == ex.SocketErrorCode || SocketError.TimedOut == ex.SocketErrorCode)
                    {
                        log.Error(Resources.NatsMessagingProvider_ConnectFailed_Fmt, i, ConnectionAttemptRetries);
                        Thread.Sleep(Seconds_10);
                        continue;
                    }
                    else
                    {
                        Status = NatsMessagingStatus.ERROR;
                        rv = false;
                        break;
                    }
                }
            }

            if (rv)
            {
                log.Debug(Resources.NatsMessagingProvider_ConnectSuccess_Fmt, host, port);
                SendConnectMessage();
            }
            else
            {
                log.Fatal(Resources.NatsMessagingProvider_ConnectionFailed_Fmt, host, port);
            }

            return rv;
        }

        private void DoPublish(string subject, Message message, uint delay = 0)
        {
            if (Message.RECEIVE_ONLY == subject)
            {
                throw new InvalidOperationException(Resources.NatsMessagingProvider_PublishReceiveOnlyMessage);
            }

            if (NatsMessagingStatus.RUNNING != Status)
            {
                return;
            }

            log.Debug(Resources.NatsMessagingProvider_PublishMessage_Fmt, subject, delay, message);
            string formattedMessage = NatsCommand.FormatPublishMessage(subject, message);
            log.Trace(Resources.NatsMessagingProvider_LogSent_Fmt, formattedMessage);

            if (delay == 0)
            {
                Write(formattedMessage);
            }
            else
            {
                Timer delayTimer = null;
                try
                {
                    delayTimer = new Timer(delay) { AutoReset = false };
                    delayTimer.Elapsed += (object sender, ElapsedEventArgs args) => Write(formattedMessage);
                    delayTimer.Enabled = true;
                    delayTimer = null;
                }
                finally
                {
                    if (delayTimer != null)
                    {
                        delayTimer.Close();
                    }
                }
            }
        }

        private void SendSubscription(NatsSubscription subscription)
        {
            log.Debug(Resources.NatsMessagingProvider_SubscribingToSubject_Fmt, subscription, subscription.Sequence);
            string formattedMessage = NatsCommand.FormatSubscribeMessage(subscription, subscription.Sequence);
            log.Trace(Resources.NatsMessagingProvider_LogSent_Fmt, formattedMessage);
            Write(formattedMessage);
        }

        private void MessageProcessor()
        {
            while (NatsMessagingStatus.RUNNING == Status)
            {
                messageQueuedEvent.WaitOne(Seconds_5);

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
                    log.Trace(Resources.NatsMessagingProvider_LogReceived_Fmt, message);

                    if (NatsCommand.Ok.Command == message)
                    {
                        log.Trace(Resources.NatsMessagingProvider_MessageAck_Fmt, message);
                    }
                    else if (message.StartsWith(NatsCommand.Information.Command))
                    {
                        log.Trace(Resources.NatsMessagingProvider_InfoMessage_Fmt, message);
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
                                }
                            }
                            if (false == messageContinuation.IsNullOrWhiteSpace())
                            {
                                break;
                            }
                            messageQueuedEvent.WaitOne(Seconds_5);
                        }

                        log.Trace(Resources.NatsMessagingProvider_LogReceived_Fmt, messageContinuation);

                        var receivedMessage = new ReceivedMessage(log, message, messageContinuation);
                        if (receivedMessage.IsValid)
                        {
                            if (false == subscriptionsByName.ContainsKey(receivedMessage.Subject))
                            {
                                log.Debug(Resources.NatsMessagingProvider_NonSubscribedSubject_Fmt, receivedMessage.Subject, receivedMessage.SubscriptionID, receivedMessage.RawMessage);
                                continue;
                            }

                            NatsSubscription natsSubscription = subscriptionsByName[receivedMessage.Subject];
                            var subjectCollection = subscriptions[natsSubscription];
                            if (false == subjectCollection.ContainsKey(receivedMessage.SubscriptionID))
                            {
                                log.Debug(Resources.NatsMessagingProvider_NoMessageSubscribers_Fmt, receivedMessage.Subject, receivedMessage.SubscriptionID, receivedMessage.RawMessage);
                                continue;
                            }

                            if (NatsMessagingStatus.RUNNING == Status)
                            {
                                Action<string, string> callback = subjectCollection[receivedMessage.SubscriptionID];
                                try
                                {
                                    callback(receivedMessage.RawMessage, receivedMessage.InboxID);
                                }
                                catch (Exception ex)
                                {
                                    log.Error(ex, Resources.NatsMessagingProvider_ExceptionInCallbackForSubscription_Fmt, natsSubscription.ToString());
                                }
                            }
                        }
                        else
                        {
                            log.Error(Resources.NatsMessagingProvider_InvalidMessage_Fmt, receivedMessage.ToString());
                        }
                    }
                }
            }
        }

        // NB: this allows "break on exceptions" to be enabled in VS without having that IOException break all the time
        [System.Diagnostics.DebuggerStepThrough]
        private void Poll()
        {
            byte[] readBuffer = new byte[tcpClient.ReceiveBufferSize];

            while (NatsMessagingStatus.RUNNING == Status)
            {
                var incomingDataSB = new StringBuilder();
                bool interrupted = false, disconnected = false;
                try
                {
                    do
                    {
                        int bytesRead = tcpClient.Read(readBuffer);
                        incomingDataSB.Append(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                    }
                    while (tcpClient.DataAvailable());
                }
                catch (InvalidOperationException)
                {
                    disconnected = true;
                }
                catch (IOException ex)
                {
                    interrupted = HandleException(ex, SocketError.Interrupted);
                    disconnected = false == interrupted;
                }

                if (disconnected && false == shuttingDown)
                {
                    log.Error(Resources.NatsMessagingProvider_Disconnected_Message);
                    if (false == Reconnect())
                    {
                        log.Fatal(Resources.NatsMessagingProvider_CouldNotReconnect_Message);
                        Status = NatsMessagingStatus.ERROR;
                        break;
                    }
                }

                if (interrupted && shuttingDown)
                {
                    // Blocking call was canceled
                    break;
                }

                if (NatsMessagingStatus.RUNNING == Status && incomingDataSB.Length > 0)
                {
                    string incomingData = incomingDataSB.ToString();
                    if (incomingData.Contains(CRLF)) // CRLF == at least one message
                    {
                        // Read a complete message
                        string[] messages = incomingData.Split(new[] { CRLF }, StringSplitOptions.RemoveEmptyEntries);
                        if (false == incomingData.EndsWith(CRLF))
                        {
                            incomingData = messages.Last(); // Last bit of data is incomplete, preserve
                            for (uint i = 0; i < messages.Length; ++i) // NB: will leave off the last partial message
                            {
                                if (false == String.IsNullOrWhiteSpace(messages[i]))
                                {
                                    lock (messageQueue)
                                    {
                                        messageQueue.Enqueue(messages[i]);
                                        messageQueuedEvent.Set();
                                    }
                                }
                            }
                        }
                        else
                        {
                            incomingData = String.Empty;
                            foreach (string msg in messages.Where(msg => false == String.IsNullOrWhiteSpace(msg)))
                            {
                                lock (messageQueue)
                                {
                                    messageQueue.Enqueue(msg);
                                    messageQueuedEvent.Set();
                                }
                            }
                        }
                    }
                }
            }
            messageQueuedEvent.Set();
        }

        private void CloseNetworking()
        {
            if (null != tcpClient)
            {
                try
                {
                    tcpClient.CloseStream();
                    tcpClient.Close();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        private void Write(string message)
        {
            if (NatsMessagingStatus.RUNNING == Status)
            {
                bool written = false;
                try
                {
                    Byte[] data = ASCIIEncoding.ASCII.GetBytes(message);
                    tcpClient.Write(data);
                    /*
                     * NB: Flush () not necessary
                     * http://msdn.microsoft.com/en-us/library/system.net.sockets.networkstream.flush.aspx
                     */
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    log.Error(ex, Resources.NatsMessagingProvider_ExceptionSendingMessage_Fmt, message);
                }
                catch (IOException ex)
                {
                    log.Error(ex, Resources.NatsMessagingProvider_ExceptionSendingMessage_Fmt, message);
                }

                if (false == written)
                {
                    log.Error(Resources.NatsMessagingProvider_Disconnected_Message);
                    messagesPendingWrite.Enqueue(message);
                }
            }
        }

        private void SendConnectMessage()
        {
            var message = new Connect
            {
                Verbose  = false,
                Pedantic = false,
                User     = String.Empty,
                Password = String.Empty,
            };
            string msgstr = NatsCommand.FormatConnectMessage(message);
            log.Debug(Resources.NatsMessagingProvider_PublishConnect_Fmt, msgstr);
            Write(msgstr);
        }

        private bool HandleException(IOException ex, SocketError error)
        {
            bool errorMatched = false;

            SocketException inner = ex.InnerException as SocketException;
            if (null != inner)
            {
                SocketError socketError = inner.SocketErrorCode;
                if (error == socketError)
                {
                    errorMatched = true;
                }
            }

            return errorMatched;
        }

        private class ReceivedMessage
        {
            /*
             * From ruby NATS:
               MSG      = /\AMSG\s+([^\s]+)\s+([^\s]+)\s+(([^\s]+)[^\S\r\n]+)?(\d+)\r\n/i #:nodoc:
               OK       = /\A\+OK\s*\r\n/i #:nodoc:
               ERR      = /\A-ERR\s+('.+')?\r\n/i #:nodoc:
               PING     = /\APING\s*\r\n/i #:nodoc:
               PONG     = /\APONG\s*\r\n/i #:nodoc:
               INFO     = /\AINFO\s+([^\r\n]+)\r\n/i  #:nodoc:
               UNKNOWN  = /\A(.*)\r\n/  #:nodoc:
             */
            private static readonly Regex stdMsg = new Regex(@"\AMSG\s+([^\s]+)\s+([^\s]+)\s+(([^\s]+)[^\S\r\n]+)?(\d+)\r\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            private readonly bool isValid = false;

            private readonly string message;

            private readonly int subscriptionID;
            private readonly int size;
            private readonly string inboxID;
            private readonly string subject;
            private readonly string rawMessage;

            public ReceivedMessage(ILog log, string messageStart, string messageContinuation)
            {
                message = String.Format("{0}\r\n{1}", messageStart, messageContinuation);
                if (stdMsg.IsMatch(message))
                {
                    Match match = stdMsg.Match(message);
                    GroupCollection groups = match.Groups;
                    if (groups.Count > 0)
                    {
                        subject = groups[1].Value;
                        inboxID = groups[4].Value;
                        if (Int32.TryParse(groups[2].Value, out subscriptionID))
                        {
                            if (Int32.TryParse(groups[5].Value, out size))
                            {
                                isValid = true;
                                rawMessage = stdMsg.Replace(message, String.Empty);
                            }
                        }
                    }
                }
                else
                {
                    log.Debug(Resources.NatsMessagingProvider_UnknownReceivedMessage_Fmt, message);
                }
            }

            public bool IsValid
            {
                get { return isValid; }
            }

            public string RawMessage
            {
                get { return rawMessage; }
            }

            public string Subject
            {
                get { return subject; }
            }

            public int SubscriptionID
            {
                get { return subscriptionID; }
            }

            public string InboxID
            {
                get { return inboxID; }
            }

            public override string ToString()
            {
                return message;
            }
        }
    }
}