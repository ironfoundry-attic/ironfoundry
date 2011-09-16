namespace CloudFoundry.Net.Dea.Providers
{
    public abstract class NatsCommand
    {
        private static class NatsCommands
        {
            public const string Ok          = "+OK";
            public const string Error       = "-ERR";
            public const string Ping        = "PING";
            public const string Pong        = "PONG";
            public const string Information = "INFO";
            public const string Message     = "MSG";
            public const string Publish     = "PUB";
            public const string Subscribe   = "SUB";
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

        public class OKCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Ok; }
            }
        }

        public class ErrorCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Error; }
            }
        }

        public class PingCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Ping; }
            }
        }

        public class PongCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Pong; }
            }
        }

        public class InformationCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Information; }
            }
        }

        public class MessageCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Message; }
            }
        }

        public class PublishCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Publish; }
            }
        }

        public class SubscribeCommand : NatsCommand
        {
            public override string Command
            {
                get { return NatsCommands.Subscribe; }
            }
        }
    }
}