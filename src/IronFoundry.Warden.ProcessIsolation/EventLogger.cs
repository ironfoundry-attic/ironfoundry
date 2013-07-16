namespace IronFoundry.Warden.ProcessIsolation
{
    using System;
    using System.Diagnostics;
    using NLog;

    public class EventLogger
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public const string EventLogName = "Application";
        public const string MachineName = ".";

        private readonly EventLog TheEventLog;

        public static EventLogger ProcessHost { get; private set; }

        static EventLogger()
        {
            ProcessHost = Create("PicoSvc");
        }

        private EventLogger(string serviceName)
        {
            if (false == EventLog.SourceExists(serviceName))
            {
                EventLog.CreateEventSource(serviceName, EventLogName);
            }
            else
            {
                if (EventLog.LogNameFromSourceName(serviceName, MachineName) != EventLogName)
                {
                    EventLog.DeleteEventSource(serviceName);
                    EventLog.CreateEventSource(serviceName, EventLogName);
                }
            }

            TheEventLog = new EventLog(EventLogName, MachineName, serviceName);
        }

        public static EventLogger Create(string serviceName)
        {
            try
            {
                return new EventLogger(serviceName);
            }
            catch (Exception ex)
            {
                log.ErrorException("Unable to create event log", ex);
                return null;
            }
        }

        public void Error(string formatMessage, params object[] args)
        {
            if (TheEventLog != null)
            {
                TheEventLog.WriteEntry(String.Format(formatMessage, args), EventLogEntryType.Error);
            }
        }

        public void Error(string message, object data)
        {
            var stringData = data == null ? "No Data" : data.ToString();

            if (TheEventLog != null)
            {
                TheEventLog.WriteEntry(message, EventLogEntryType.Error, 1, 1, System.Text.Encoding.UTF8.GetBytes(stringData));
            }
        }

        public void Error(string message, Exception ex)
        {
            if (TheEventLog != null)
            {
                TheEventLog.WriteEntry(message, EventLogEntryType.Error, 1, 1, System.Text.Encoding.UTF8.GetBytes(ex.ToString()));
            }
        }

        public void Warn(string message, Exception ex)
        {
            if (TheEventLog != null)
            {
                TheEventLog.WriteEntry(message, EventLogEntryType.Warning, 1, 2, System.Text.Encoding.UTF8.GetBytes(ex.ToString()));
            }
        }

        public void Warn(string message, params object[] args)
        {
            if (TheEventLog != null)
            {
                TheEventLog.WriteEntry(string.Format(message, args), EventLogEntryType.Warning);
            }
        }

        public void Info(string message, params object[] args)
        {
            if (TheEventLog != null)
            {
                TheEventLog.WriteEntry(string.Format(message, args), EventLogEntryType.Information);
            }
        }
    }
}
