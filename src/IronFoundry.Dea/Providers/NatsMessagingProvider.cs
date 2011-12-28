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
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Types;

    public class NatsMessagingProvider : IMessagingProvider
    {
        private static readonly ushort ConnectionAttemptRetries = 10;

        private readonly TimeSpan WaitOneTimeSpan = TimeSpan.FromMilliseconds(250);

        private const string CRLF = "\r\n";

        private readonly string host;
        private readonly ushort port;
        private readonly IDictionary<string, IDictionary<int, Action<string, string>>> subscriptions
            = new Dictionary<string, IDictionary<int, Action<string, string>>>();
        private readonly Guid uniqueIdentifier;

        private int sequence = 1;
        private bool shutting_down = false;
        private bool error_occurred = false;

        private TcpClient tcpClient;
        private NetworkStream networkStream;

        private readonly Queue<string> messageQueue = new Queue<string>();
        private readonly AutoResetEvent messageQueuedEvent = new AutoResetEvent(false);

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
            log.Debug(Resources.NatsMessagingProvider_Initialized_Fmt, UniqueIdentifier, host, port);

            Status = NatsMessagingStatus.RUNNING;
        }

        public NatsMessagingStatus Status { get; private set; }

        public string StatusMessage { get; private set; }

        public Guid UniqueIdentifier { get { return uniqueIdentifier; } }

        public int Sequence { get { return sequence; } }

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

            Interlocked.Increment(ref sequence);

            log.Debug(Resources.NatsMessagingProvider_SubscribingToSubject_Fmt, subscription, Sequence);

            string formattedMessage = NatsCommand.FormatSubscribeMessage(subscription, Sequence);

            log.Trace(Resources.NatsMessagingProvider_LogSent_Fmt, formattedMessage);

            Write(formattedMessage);

            lock (subscriptions)
            {
                if (false == subscriptions.ContainsKey(subscription.Subscription))
                {
                    subscriptions.Add(subscription.Subscription, new Dictionary<int, Action<string, string>>());
                }
                subscriptions[subscription.Subscription].Add(Sequence, callback);
            }
        }

        public bool Connect()
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
                    tcpClient = new TcpClient(host, port) { NoDelay = true };
                    networkStream = tcpClient.GetStream();
                    if (networkStream.CanTimeout)
                    {
                        networkStream.ReadTimeout = (int)WaitOneTimeSpan.TotalMilliseconds;
                    }
                    rv = true;
                }
                catch (SocketException ex)
                {
                    if (SocketError.ConnectionRefused == ex.SocketErrorCode || SocketError.TimedOut == ex.SocketErrorCode)
                    {
                        log.Error(Resources.NatsMessagingProvider_ConnectFailed_Fmt, i, ConnectionAttemptRetries);
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

        public void Start()
        {
            if (NatsMessagingStatus.RUNNING == Status)
            {
                pollTask = Task.Factory.StartNew(Poll);
                messageProcessorTask = Task.Factory.StartNew(MessageProcessor);
            }
        }

        public void Stop()
        {
            if (shutting_down)
            {
                throw new InvalidOperationException(Resources.NatsMessagingProvider_AttemptingStopTwice_Message);
            }

            Status = NatsMessagingStatus.STOPPING;
            shutting_down = true;

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
            CloseNetworking();
            log.Debug(Resources.NatsMessagingProvider_Disconnected_Message);

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

        private void DoPublish(string subject, Message message)
        {
            if (Message.RECEIVE_ONLY == subject)
            {
                throw new InvalidOperationException(Resources.NatsMessagingProvider_PublishReceiveOnlyMessage);
            }

            if (NatsMessagingStatus.RUNNING != Status)
            {
                return;
            }

            log.Debug(Resources.NatsMessagingProvider_PublishMessage_Fmt, subject, message);
            string formattedMessage = NatsCommand.FormatPublishMessage(subject, message);
            log.Trace(Resources.NatsMessagingProvider_LogSent_Fmt, formattedMessage);
            Write(formattedMessage);
        }

        private void MessageProcessor()
        {
            while (NatsMessagingStatus.RUNNING == Status)
            {
                messageQueuedEvent.WaitOne(WaitOneTimeSpan);

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
                            messageQueuedEvent.WaitOne(WaitOneTimeSpan);
                        }

                        log.Trace(Resources.NatsMessagingProvider_LogReceived_Fmt, messageContinuation);

                        var receivedMessage = new ReceivedMessage(message + CRLF + messageContinuation);

                        if (false == subscriptions.ContainsKey(receivedMessage.Subject))
                        {
                            log.Debug(Resources.NatsMessagingProvider_NonSubscribedSubject_Fmt, receivedMessage.Subject, receivedMessage.SubscriptionID, receivedMessage.RawMessage);
                            continue;
                        }

                        var subjectCollection = subscriptions[receivedMessage.Subject];
                        if (false == subjectCollection.ContainsKey(receivedMessage.SubscriptionID))
                        {
                            log.Debug(Resources.NatsMessagingProvider_NoMessageSubscribers_Fmt, receivedMessage.Subject, receivedMessage.SubscriptionID, receivedMessage.RawMessage);
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
            byte[] readBuffer = new byte[tcpClient.ReceiveBufferSize];

            while (NatsMessagingStatus.RUNNING == Status)
            {
                var incomingDataSB = new StringBuilder();
                if (networkStream.CanRead)
                {
                    try
                    {
                        do
                        {
                            int bytesRead = networkStream.Read(readBuffer, 0, readBuffer.Length);
                            incomingDataSB.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
                        } while (networkStream.DataAvailable);
                    }
                    catch (IOException ex)
                    {
                        HandleException(ex, SocketError.TimedOut);
                    }
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
                    networkStream.Close();
                    networkStream.Dispose();
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
            while (NatsMessagingStatus.RUNNING == Status)
            {
                try
                {
                    Byte[] data = ASCIIEncoding.ASCII.GetBytes(message);
                    networkStream.Write(data, 0, data.Length);
                    /*
                     * NB: Flush () not necessary
                     * http://msdn.microsoft.com/en-us/library/system.net.sockets.networkstream.flush.aspx
                     */
                    return;
                }
                catch (IOException ex)
                {
                    log.Error(ex, Resources.NatsMessagingProvider_ExceptionSendingMessage_Fmt, message);
                    HandleException(ex);
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

        private void HandleException(IOException ex, params SocketError[] okErrors)
        {
            if (okErrors.IsNullOrEmpty())
            {
                OnFatalException(ex);
            }
            else
            {
                SocketException inner = ex.InnerException as SocketException;
                if (null == inner)
                {
                    OnFatalException(ex);
                }
                else
                {
                    SocketError socketError = inner.SocketErrorCode;
                    if (false == okErrors.Contains(socketError))
                    {
                        OnFatalException(ex);
                    }
                }
            }
        }

        private void OnFatalException(Exception ex)
        {
            error_occurred = true;
            Status = NatsMessagingStatus.ERROR;
            StatusMessage = ex.Message;
            log.Error(ex);
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
