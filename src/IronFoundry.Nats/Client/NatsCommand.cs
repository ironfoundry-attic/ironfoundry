namespace IronFoundry.Nats.Client
{
    using System;
    using System.Globalization;
    using System.Text;

    public abstract class NatsCommand
    {
        private static class NatsCommands
        {
            public const string ok          = "+OK";
            public const string error       = "-ERR";
            public const string ping        = "PING";
            public const string pong        = "PONG";
            public const string information = "INFO";
            public const string message     = "MSG";
            public const string publish     = "PUB";
            public const string subscribe   = "SUB";
            public const string connect     = "CONNECT";
        }

        private static class NatsCommandFormats
        {
            public static readonly string ConnectFmt = NatsCommand.Connect.Command + " {0}\r\n";

            /// <summary>
            /// Format for sending publish messages. First parameter is the subject,
            /// second parameter is the actual byte length of the message (you should
            /// retrieve this by converting the string to an ascii message than retrieving
            /// the byte array length that's produced),
            /// third parameter is the actual message (should be in JSON format).
            /// </summary>
            public static readonly string PublishFmt = NatsCommand.Publish.Command + " {0}  {1}\r\n{2}\r\n";

            /// <summary>
            /// Format for sending subscribe message. First parameter is the subject,
            /// second parameter is a sequential integer identifier. For every subscribe message
            /// a running tally of unique integers is used in order to reply back to the
            /// subscribed message.
            /// </summary>
            public static readonly string SubscribeFmt = NatsCommand.Subscribe.Command + " {0}  {1}\r\n";
        }

        public static readonly NatsCommand Ok          = new OKCommand();
        public static readonly NatsCommand Error       = new ErrorCommand();
        public static readonly NatsCommand Ping        = new PingCommand();
        public static readonly NatsCommand Pong        = new PongCommand();
        public static readonly NatsCommand Information = new InformationCommand();
        public static readonly NatsCommand Message     = new MessageCommand();
        public static readonly NatsCommand Publish     = new PublishCommand();
        public static readonly NatsCommand Subscribe   = new SubscribeCommand();
        public static readonly NatsCommand Connect     = new ConnectCommand();

        public abstract string Command { get; }

        public static string FormatSubscribeMessage(INatsSubscription subscription, int Sequence)
        {
            return String.Format(CultureInfo.InvariantCulture, NatsCommandFormats.SubscribeFmt, subscription, Sequence);
        }

        public static string FormatPublishMessage(string subject, INatsMessage message)
        {
            string messageJson = message.ToJson();
            return FormatPublishMessage(subject, messageJson);
        }

        public static string FormatPublishMessage(string subject, string json)
        {
            return String.Format(CultureInfo.InvariantCulture, NatsCommandFormats.PublishFmt, subject, Encoding.ASCII.GetBytes(json).Length, json);
        }

        public static string FormatConnectMessage(INatsMessage message)
        {
            string messageJson = message.ToJson();
            return String.Format(CultureInfo.InvariantCulture, NatsCommandFormats.ConnectFmt, message);
        }

        private class OKCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.ok; }
            }
        }

        private class ErrorCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.error; }
            }
        }

        private class PingCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.ping; }
            }
        }

        private class PongCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.pong; }
            }
        }

        private class InformationCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.information; }
            }
        }

        private class MessageCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.message; }
            }
        }

        private class PublishCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.publish; }
            }
        }

        private class SubscribeCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.subscribe; }
            }
        }

        private class ConnectCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.connect; }
            }
        }
    }
}
