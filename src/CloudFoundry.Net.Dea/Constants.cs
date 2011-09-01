using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Dea
{
    public static class Constants
    {
        public const string JsonDateFormat = "yyyy-MM-dd HH:mm:ss zz00";
        public const string LocalhostIP = "127.0.0.1";

        public static class AppSettings
        {
            public static string StagingDirectory = "StagingDirectory";
            public static string ApplicationsDirectory = "ApplicationsDirectory";
            public static string DropletsDirectory = "DropletsDirectory";
            public static string NatsHost = "NatsHost";
            public static string NatsPort = "NatsPort";
            public static string IISHost = "IISHost";
            
        }

        public static class NatsCommands
        {
            public static string Ok = "+OK";
            public static string Error = "-ERR";
            public static string Ping = "PING";
            public static string Pong = "PONG";
            public static string Information = "INFO";
            public static string Message = "MSG";
            public static string Publish = "PUB";
            public static string Subscribe = "SUB";
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
            public static string Publish = NatsCommands.Publish + " {0}  {1}\r\n{2}\r\n";

            /// <summary>
            /// Format for sending subscribe message. First parameter is the subject,
            /// second parameter is a sequential integer identifier. For every subscribe message
            /// a running tally of unique integers is used in order to reply back to the 
            /// subscribed message.
            /// </summary>
            public static string Subscribe = NatsCommands.Subscribe + " {0}  {1}\r\n";
        }

        public static class Messages
        {
            public static string DeaStart = "dea.start";
            public static string DeaHeartbeat = "dea.heartbeat";
            public static string RouterRegister = "router.register";
            public static string RouterUnregister = "router.unregister";
            public static string DropletExited = "droplet.exited";
            public static string DeaInstanceStart = "dea.{0}.start";
            public static string DeaStop = "dea.stop";
            public static string VcapComponentAnnounce = "vcap.component.announce";
            public static string VcapComponentDiscover = "vcap.component.discover";
            public static string DeaStatus = "dea.status";
            public static string DropletStatus = "droplet.status";
            public static string DeaDiscover = "dea.discover";
            public static string DeaFindDroplet = "dea.find.droplet";
            public static string DeaUpdate = "dea.update";
            public static string RouterStart = "router.start";
            public static string HealthManagerStart = "healthmanager.start";

        }

        public static class InstanceState
        {
            public static string STARTING = "STARTING";
            public static string STOPPED = "STOPPED";
            public static string RUNNING = "RUNNING";
            public static string SHUTTING_DOWN = "SHUTTING_DOWN";
            public static string CRASHED = "CRASHED";
            public static string DELETED = "DELETED";
        }
    }
}
