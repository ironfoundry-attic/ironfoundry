namespace CloudFoundry.Net.Dea.Providers
{
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
        }

        public static readonly NatsCommand Ok          = new OKCommand();
        public static readonly NatsCommand Error       = new ErrorCommand();
        public static readonly NatsCommand Ping        = new PingCommand();
        public static readonly NatsCommand Pong        = new PongCommand();
        public static readonly NatsCommand Information = new InformationCommand();
        public static readonly NatsCommand Message     = new MessageCommand();
        public static readonly NatsCommand Publish     = new PublishCommand();
        public static readonly NatsCommand Subscribe   = new SubscribeCommand();

        public abstract string Command { get; }

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
    }
}