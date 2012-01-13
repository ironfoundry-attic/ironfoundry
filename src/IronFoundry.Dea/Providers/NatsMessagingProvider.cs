namespace IronFoundry.Dea.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
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
        private enum ParseState
        {
            AWAITING_CONTROL_LINE, // nats, client.rb, 47
            AWAITING_MSG_PAYLOAD,
        }

        private const string PongResponse = "PONG\r\n";

        private static readonly RegexOptions commonOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        private static readonly Regex MSG     = new Regex(@"\AMSG\s+([^\s]+)\s+([^\s]+)\s+(([^\s]+)[^\S\r\n]+)?(\d+)\r\n", commonOptions);
        private static readonly Regex OK      = new Regex(@"\A\+OK\s*\r\n", commonOptions);
        private static readonly Regex ERR     = new Regex(@"\A-ERR\s+('.+')?\r\n", commonOptions);
        private static readonly Regex PING    = new Regex(@"\APING\s*\r\n", commonOptions);
        private static readonly Regex PONG    = new Regex(@"\APONG\s*\r\n", commonOptions);
        private static readonly Regex INFO    = new Regex(@"\AINFO\s+([^\r\n]+)\r\n", commonOptions);
        private static readonly Regex UNKNOWN = new Regex(@"\A(.*)\r\n", commonOptions);

        private static readonly ushort ConnectionAttemptRetries = 60;

        private readonly TimeSpan Seconds_5 = TimeSpan.FromSeconds(5);
        private readonly TimeSpan Seconds_10 = TimeSpan.FromSeconds(10);

        private const string CRLF = "\r\n";
        private static readonly int CRLFLen = CRLF.Length;

        private readonly string host;
        private readonly ushort port;

        private readonly IList<NatsSubscription> subscriptions = new List<NatsSubscription>();
        private readonly IDictionary<int, IList<Action<string, string>>> subscriptionCallbacks =
            new Dictionary<int, IList<Action<string, string>>>();

        private readonly Guid uniqueIdentifier;

        private bool shuttingDown = false;

        private TcpClient tcpClient;

        private readonly Queue<ReceivedMessage> messageQueue = new Queue<ReceivedMessage>();
        private readonly AutoResetEvent messageQueuedEvent = new AutoResetEvent(false);

        private readonly Queue<string> messagesPendingWrite = new Queue<string>();

        private Task messageProcessorTask;
        private Task pollTask;

        private readonly ILog log;

        private ParseState currentParseState = ParseState.AWAITING_CONTROL_LINE;

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
                if (false == subscriptions.Contains(subscription))
                {
                    subscriptions.Add(subscription);
                }

                int subscriptionID = subscription.SubscriptionID;
                if (false == subscriptionCallbacks.ContainsKey(subscriptionID))
                {
                    subscriptionCallbacks.Add(subscriptionID, new List<Action<string, string>>());
                }
                subscriptionCallbacks[subscriptionID].Add(callback);
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
                    foreach (NatsSubscription subscription in subscriptions)
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
                currentParseState = ParseState.AWAITING_CONTROL_LINE;
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
            log.Debug(Resources.NatsMessagingProvider_SubscribingToSubject_Fmt, subscription, subscription.SubscriptionID);
            string formattedMessage = NatsCommand.FormatSubscribeMessage(subscription, subscription.SubscriptionID);
            log.Trace(Resources.NatsMessagingProvider_LogSent_Fmt, formattedMessage);
            Write(formattedMessage);
        }

        private void MessageProcessor()
        {
            while (NatsMessagingStatus.RUNNING == Status)
            {
                messageQueuedEvent.WaitOne(Seconds_5);

                ReceivedMessage message = null;
                lock (messageQueue)
                {
                    if (false == messageQueue.IsNullOrEmpty())
                    {
                        message = messageQueue.Dequeue();
                    }
                }

                if (null != message)
                {
                    log.Trace(Resources.NatsMessagingProvider_LogReceived_Fmt, message);

                    if (false == subscriptionCallbacks.ContainsKey(message.SubscriptionID))
                    {
                        log.Debug(Resources.NatsMessagingProvider_NonSubscribedSubject_Fmt,
                            message.Subject, message.SubscriptionID, message.Message);
                        continue;
                    }

                    if (NatsMessagingStatus.RUNNING == Status)
                    {
                        IList<Action<string, string>> callbacks = subscriptionCallbacks[message.SubscriptionID];
                        if (false == callbacks.IsNullOrEmpty())
                        {
                            foreach (var callback in callbacks)
                            {
                                try
                                {
                                    callback(message.Message, message.InboxID);
                                }
                                catch (Exception ex)
                                {
                                    log.Error(ex, Resources.NatsMessagingProvider_ExceptionInCallbackForSubscription_Fmt, message.Subject);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Poll()
        {
            byte[] readBuffer = new byte[tcpClient.ReceiveBufferSize];

            // These are for the current message
            string subject = null;
            int subscriptionID = 0;
            int needed = 0;
            string inboxID = null;
            StringBuilder messageBuffer = null;

            while (NatsMessagingStatus.RUNNING == Status)
            {

            ReceiveMoreData:

                bool interrupted = false, disconnected = false;

                if (null == messageBuffer)
                {
                    messageBuffer = new StringBuilder();
                }

                try
                {
                    do
                    {
                        int bytesRead = tcpClient.Read(readBuffer);
                        messageBuffer.Append(Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
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

                if (NatsMessagingStatus.RUNNING == Status && messageBuffer.Length > 0)
                {
                    while (null != messageBuffer)
                    {
                        string incomingData = messageBuffer.ToString();
                        log.Trace("Parsing: '{0}'", incomingData);

                        switch (currentParseState)
                        {
                            case ParseState.AWAITING_CONTROL_LINE:
                                {
                                    if (MSG.IsMatch(incomingData))
                                    {
                                        Match match = MSG.Match(incomingData);
                                        incomingData = match.Postmatch(incomingData);
                                        GroupCollection groups = match.Groups;
                                        if (groups.Count > 0)
                                        {
                                            subject = groups[1].Value;
                                            subscriptionID = Convert.ToInt32(groups[2].Value, CultureInfo.InvariantCulture);
                                            inboxID = groups[4].Value;
                                            needed = Convert.ToInt32(groups[5].Value, CultureInfo.InvariantCulture);
                                            currentParseState = ParseState.AWAITING_MSG_PAYLOAD;
                                        }
                                    }
                                    else if (OK.IsMatch(incomingData))
                                    {
                                        Match match = OK.Match(incomingData);
                                        incomingData = match.Postmatch(incomingData);
                                    }
                                    else if (ERR.IsMatch(incomingData))
                                    {
                                        Match match = ERR.Match(incomingData);
                                        incomingData = match.Postmatch(incomingData);
                                        GroupCollection groups = match.Groups;
                                        if (groups.Count > 0)
                                        {
                                            string errorData = match.Groups[1].Value;
                                            log.Info(Resources.NatsMessagingProvider_NatsErrorReceived_Fmt, errorData);
                                        }
                                    }
                                    else if (PING.IsMatch(incomingData))
                                    {
                                        Write(PongResponse);
                                        Match match = PING.Match(incomingData);
                                        incomingData = match.Postmatch(incomingData);
                                    }
                                    else if (PONG.IsMatch(incomingData))
                                    {
                                        // TODO: callbacks?
                                        Match match = PONG.Match(incomingData);
                                        incomingData = match.Postmatch(incomingData);
                                    }
                                    else if (INFO.IsMatch(incomingData))
                                    {
                                        Match match = INFO.Match(incomingData);
                                        incomingData = match.Postmatch(incomingData);
                                        GroupCollection groups = match.Groups;
                                        if (groups.Count > 0)
                                        {
                                            string infoData = groups[1].Value;
                                            log.Info(Resources.NatsMessagingProvider_NatsInfoReceived_Fmt, infoData);
                                        }
                                    }
                                    else if (UNKNOWN.IsMatch(incomingData))
                                    {
                                        Match match = UNKNOWN.Match(incomingData);
                                        incomingData = match.Postmatch(incomingData);
                                        log.Error(Resources.NatsMessagingProvider_NatsUnknownReceived_Fmt, match.Value);
                                    }
                                    else
                                    {
                                        // If we are here we do not have a complete line yet that we understand.
                                        goto ReceiveMoreData;
                                    }

                                    messageBuffer.Clear();
                                    messageBuffer.Append(incomingData);
                                    if (0 == messageBuffer.Length)
                                    {
                                        messageBuffer = null;
                                    }
                                }
                                break;
                            case ParseState.AWAITING_MSG_PAYLOAD:
                                {
                                    if (messageBuffer.Length < (needed + CRLFLen))
                                    {
                                        goto ReceiveMoreData;
                                    }
                                    else
                                    {
                                        string message = messageBuffer.ToString(0, needed);

                                        var receivedMessage = new ReceivedMessage(subject, subscriptionID, inboxID, message);
                                        lock (messageQueue)
                                        {
                                            messageQueue.Enqueue(receivedMessage);
                                            messageQueuedEvent.Set();
                                        }

                                        int startIndex = needed + CRLFLen;
                                        int length = messageBuffer.Length - startIndex;
                                        string remaining = messageBuffer.ToString(startIndex, length);

                                        if (remaining.Length > 0)
                                        {
                                            messageBuffer = new StringBuilder(remaining);
                                        }
                                        else
                                        {
                                            messageBuffer = null;
                                        }

                                        // NB: do resets last
                                        inboxID = String.Empty;
                                        subscriptionID = 0;
                                        needed = 0;
                                        currentParseState = ParseState.AWAITING_CONTROL_LINE;
                                    }
                                }
                                break;
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
            private readonly string subject;
            private readonly int subscriptionID;
            private readonly string inboxID;
            private readonly string message;

            public ReceivedMessage(string subject, int subscriptionID, string inboxID, string message)
            {
                this.subject        = subject;
                this.subscriptionID = subscriptionID;
                this.inboxID        = inboxID;
                this.message        = message;
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

            public string Message
            {
                get { return message; }
            }

            public override string ToString()
            {
                return String.Format("Subject: {0} SubID: {1} InboxID: {2} Message: {3}", subject, subscriptionID, inboxID, message);
            }
        }
    }
}