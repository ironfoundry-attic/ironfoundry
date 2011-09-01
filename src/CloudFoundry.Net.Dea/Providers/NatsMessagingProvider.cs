using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using NLog;
using System.Threading;
using CloudFoundry.Net.Dea.Providers.Interfaces;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CloudFoundry.Net.Dea.Providers
{    
    public class NatsMessagingProvider : IMessagingProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string LogSentFormat = "NATS Msg Sent: {0}";
        private const string LogReceivedFormat = "NATS Msg Recv: {0}";
        private const string CRLF = "\r\n";

        public string Host { get; private set; }
        public int Port { get; private set; }
        public string UniqueIdentifier { get; private set; }
        public int Sequence { get; private set; }
        public bool CurrentlyPolling { get; private set; }
        private TcpClient client;
        private NetworkStream stream;
        private Dictionary<string,Dictionary<int, Action<string,string>>>Subscriptions;
        private Object lockObject = new Object();
        private bool disposing = false;
        

        public NatsMessagingProvider(string host, int port)
        {
            Host = host;
            Port = port;
            Sequence = 1;
            UniqueIdentifier = Guid.NewGuid().ToString("N");   
            Logger.Debug("NATS Messaging Provider Initialized. Identifier: {0}, Server Host: {1}, Server Port: {2}.", UniqueIdentifier, Host, Port);
            Subscriptions = new Dictionary<string, Dictionary<int, Action<string,string>>>();
            CurrentlyPolling = false;
        }

        public void Publish(string subject, string message)
        {
            Logger.Debug("NATS Publishing subject: {0},{1}", subject, message);  
            var formattedMessage = string.Format(Constants.NatsCommandFormats.Publish, subject, Encoding.ASCII.GetBytes(message).Length, message);
            Logger.Trace(LogSentFormat, formattedMessage);
            Write(formattedMessage);
        }        

        public void Subscribe(string subject, Action<string,string> replyCallback)
        {
            lock (lockObject) { Sequence++; }

            Logger.Debug("NATS Subscribing to subject: {0}, sequence {1}", subject, Sequence);            
            var formattedMessage = string.Format(Constants.NatsCommandFormats.Subscribe, subject, Sequence);
            Logger.Trace(LogSentFormat,formattedMessage);
            Write(formattedMessage);
            
            lock (lockObject) { 
                if (!Subscriptions.ContainsKey(subject))
                    Subscriptions.Add(subject, new Dictionary<int,Action<string,string>>());
                Subscriptions[subject].Add(Sequence, replyCallback);
            }
        }
        
        public void Poll()
        {
            CurrentlyPolling = true;
            string response = string.Empty;
            while (!disposing)
            {
                Thread.Sleep(50);

                int receivedDataLength;
                byte[] data = new byte[1024];                
                receivedDataLength = stream.Read(data, 0, data.Length);
                stream.Flush();
                response += Encoding.ASCII.GetString(data, 0, receivedDataLength);
                if (!response.Contains(CRLF))
                    continue;
                string[] messages = Regex.Split(response, CRLF);                
                if (!response.EndsWith(CRLF))
                    response = messages[messages.Length - 1];
                else
                    response = string.Empty;

                for (int counter = 0; counter < messages.Length; counter++)
                {
                    var message = messages[counter];
                    if (String.IsNullOrEmpty(message))
                        continue;

                    Logger.Trace(LogReceivedFormat, message);

                    if (message == Constants.NatsCommands.Ok)
                    {
                        Logger.Trace("MATS Message Acknowledged: {0}", message);
                    }
                    if (message.StartsWith(Constants.NatsCommands.Information))
                    {
                        Logger.Trace("NATS Info Message {0}", message);
                    }
                    if (message.StartsWith(Constants.NatsCommands.Message))
                    {
                        var nextLine = messages[++counter];
                        var receivedMessage = new ReceivedMessage(message + CRLF + nextLine);
                        if (!Subscriptions.ContainsKey(receivedMessage.Subject))
                        {
                            Logger.Debug("NATS Message Subject: {0} not found to be subscribed. Ignoring received message {1},{2}.", receivedMessage.Subject, receivedMessage.SubscriptionID, receivedMessage.RawMessage);
                            continue;
                        }
                        var subjectCollection = Subscriptions[receivedMessage.Subject];
                        if (!subjectCollection.ContainsKey(receivedMessage.SubscriptionID))
                        {
                            Logger.Debug("NATS Message Subscription ID: {0} not found to be subscribed for subject {1}. Ignoring received message {2}.", receivedMessage.SubscriptionID, receivedMessage.Subject, receivedMessage.RawMessage);
                            continue;
                        }

                        Action<string, string> callback = subjectCollection[receivedMessage.SubscriptionID];
                        Task.Factory.StartNew(() => callback(receivedMessage.RawMessage, receivedMessage.InboxID));
                    }
                }                                              
            }
            CurrentlyPolling = false;
        }

        public void Connect()
        {            
            client = new TcpClient(Host, Port);
            Logger.Debug("NATS Connected on Host: {0}, Port: {1}", Host, Port);
            stream = client.GetStream();            
        }        

        public void Dispose()
        {
            try
            {
                disposing = true;
                Logger.Debug("NATS Waiting for polling to cease.");
                while (CurrentlyPolling) {}
                 
                Logger.Debug("NATS Disconnected.");
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
