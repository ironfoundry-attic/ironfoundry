/*
 * TODO TODO TODO
 * Have all logging go to file / output / event log
 */
namespace IronFoundry.Dea.Service
{
    using System;
    using System.Diagnostics;
    using NLog;

    public static class EventLogger
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly EventLog eventLog;

        static EventLogger()
        {
            var serviceName = "Dea Service";

            if (false == EventLog.SourceExists(serviceName))
            {
                EventLog.CreateEventSource(serviceName, "Cloud Foundry");
            }
            else
            {
                if (EventLog.LogNameFromSourceName(serviceName, ".") != "Cloud Foundry")
                {
                    EventLog.DeleteEventSource(serviceName);
                    EventLog.CreateEventSource(serviceName, "Cloud Foundry");
                }
            }

            eventLog = new EventLog("Cloud Foundry") { Source = serviceName };
        }

        public static void Error(string message)
        {
            if (eventLog != null)
            {
                eventLog.WriteEntry(message, EventLogEntryType.Error);
            }

            logger.Error(message);
        }

        public static void Error(string formatMessage, params object[] args)
        {
            if (eventLog != null)
            {
                eventLog.WriteEntry(String.Format(formatMessage, args), EventLogEntryType.Error);
            }
            logger.Error(String.Format(formatMessage, args));
        }

        public static void Error(string message, object data)
        {
            var stringData = data == null ? "No Data" : data.ToString();

            if (eventLog != null)
            {
                eventLog.WriteEntry(message, EventLogEntryType.Error, 1, 1, System.Text.Encoding.UTF8.GetBytes(stringData));
            }
            logger.Error(string.Concat(message, ": ", stringData));
        }

        public static void Error(string message, Exception ex)
        {
            if (eventLog != null)
            {
                eventLog.WriteEntry(message, EventLogEntryType.Error, 1, 1, System.Text.Encoding.UTF8.GetBytes(ex.ToString()));
            }
            logger.ErrorException(message, ex);
        }

        public static void Info(string message)
        {
            if (eventLog != null)
            {
                eventLog.WriteEntry(message, EventLogEntryType.Information);
            }
            logger.Info(message);
        }

        public static void Info(string formatMessage, params object[] args)
        {
            if (eventLog != null)
            {
                eventLog.WriteEntry(string.Format(formatMessage, args), EventLogEntryType.Information);
            }
            logger.Info(string.Format(formatMessage, args));
        }

    }
}
