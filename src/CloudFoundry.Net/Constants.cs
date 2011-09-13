namespace CloudFoundry.Net
{
    using System.Net;

    public static class Constants
    {
        public const string JsonDateFormat = "yyyy-MM-dd HH:mm:ss zz00";
        public static readonly IPAddress LocalhostIP;

        private const string localhostStr = "127.0.0.1";

        static Constants()
        {
            IPAddress.TryParse(localhostStr, out LocalhostIP);
        }

        public static class AppSettings
        {
            public const string StagingDirectory = "StagingDirectory";
            public const string ApplicationsDirectory = "ApplicationsDirectory";
            public const string DropletsDirectory = "DropletsDirectory";
            public const string NatsHost = "NatsHost";
            public const string NatsPort = "NatsPort";
        }

        public static class NatsCommands
        {
            public const string Ok = "+OK";
            public const string Error = "-ERR";
            public const string Ping = "PING";
            public const string Pong = "PONG";
            public const string Information = "INFO";
            public const string Message = "MSG";
            public const string Publish = "PUB";
            public const string Subscribe = "SUB";
        }

        public static class NatsCommandFormats
        {
            /// <summary>
            /// Format for sending publish messages. First parameter is the subject,
            /// second parameter is the actual byte length of the message (you should
            /// retrieve this by converting the string to an ascii message than retrieving 
            /// the byte array length that's produced), 
            /// third parameter is the actual message (should be in JSON format).
            /// </summary>
            public const string Publish = NatsCommands.Publish + " {0}  {1}\r\n{2}\r\n";

            /// <summary>
            /// Format for sending subscribe message. First parameter is the subject,
            /// second parameter is a sequential integer identifier. For every subscribe message
            /// a running tally of unique integers is used in order to reply back to the 
            /// subscribed message.
            /// </summary>
            public const string Subscribe = NatsCommands.Subscribe + " {0}  {1}\r\n";
        }

        public static class Messages
        {
            public const string DeaStart = "dea.start";
            public const string DeaHeartbeat = "dea.heartbeat";
            public const string RouterRegister = "router.register";
            public const string RouterUnregister = "router.unregister";
            public const string DropletExited = "droplet.exited";
            public const string DeaInstanceStart = "dea.{0}.start";
            public const string DeaStop = "dea.stop";
            public const string VcapComponentAnnounce = "vcap.component.announce";
            public const string VcapComponentDiscover = "vcap.component.discover";
            public const string DeaStatus = "dea.status";
            public const string DropletStatus = "droplet.status";
            public const string DeaDiscover = "dea.discover";
            public const string DeaFindDroplet = "dea.find.droplet";
            public const string DeaUpdate = "dea.update";
            public const string RouterStart = "router.start";
            public const string HealthManagerStart = "healthmanager.start";

        }
    }
}
