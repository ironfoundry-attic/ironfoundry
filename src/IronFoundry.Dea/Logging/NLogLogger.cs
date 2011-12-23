namespace IronFoundry.Dea.Logging
{
    using System;
    using System.Collections.Generic;
    using IronFoundry.Dea.Properties;
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    public class NLogLogger : ILog
    {
        private const string MachineName = ".";
        private const string Source = "IronFoundry.Dea.Service"; // NB: must sync with installer
        private const string LogName = "Iron Foundry";

        private static readonly IDictionary<LogLevel, ushort> eventIDMap = new Dictionary<LogLevel, ushort>
        {
            { LogLevel.Trace, 1000 },
            { LogLevel.Debug, 1001 },
            { LogLevel.Info,  1002 },
            { LogLevel.Warn,  1003 },
            { LogLevel.Error, 1004 },
            { LogLevel.Fatal, 1005 },
        };

        private readonly Logger logger;

        static NLogLogger()
        {
            addEventLogger();
        }

        public NLogLogger(string name)
        {
            logger = LogManager.GetLogger(name);
        }

        public void Debug(string fmt, params object[] args)
        {
            if (logger.IsDebugEnabled)
            {
                log(LogLevel.Debug, fmt, args);
            }
        }

        public void Error(string fmt, params object[] args)
        {
            if (logger.IsErrorEnabled)
            {
                log(LogLevel.Error, fmt, args);
            }
        }

        public void Error(Exception ex, string fmt, params object[] args)
        {
            if (logger.IsErrorEnabled)
            {
                log(LogLevel.Error, ex, fmt, args);
            }
        }

        public void Error(Exception ex)
        {
            if (logger.IsErrorEnabled)
            {
                log(LogLevel.Error, ex);
            }
        }

        public void Fatal(string fmt, params object[] args)
        {
            if (logger.IsFatalEnabled)
            {
                log(LogLevel.Fatal, fmt, args);
            }
        }

        public void Info(string fmt, params object[] args)
        {
            if (logger.IsInfoEnabled)
            {
                log(LogLevel.Info, fmt, args);
            }
        }

        public void Trace(string fmt, params object[] args)
        {
            if (logger.IsTraceEnabled)
            {
                log(LogLevel.Trace, fmt, args);
            }
        }

        public void Warn(string fmt, params object[] args)
        {
            if (logger.IsWarnEnabled)
            {
                log(LogLevel.Warn, fmt, args);
            }
        }

        private void log(LogLevel level, string fmt, params object[] args)
        {
            logit(new LogEventInfo(level, logger.Name, null, fmt, args));
        }

        private void log(LogLevel level, Exception ex)
        {
            log(level, ex, null, null);
        }

        private void log(LogLevel level, Exception ex, string fmt, params object[] args)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }
            logit(new LogEventInfo(level, logger.Name, null, fmt, args, ex));
        }

        private void logit(LogEventInfo lei)
        {
            // set the per-log context data
            // this data can be retrieved using ${event-context:item=EventID}
            lei.Properties["EventID"] = eventIDMap[lei.Level];
            // log the message
            logger.Log(typeof(NLogLogger), lei);
        }

        private static void addEventLogger()
        {
            try
            {
                bool sourceExists = false;

                /*
                 * TODO
                if (EventLog.SourceExists(Source))
                {
                    sourceExists = true;
                }
                else
                {
                    sourceExists = true;
                }
                 */

                if (sourceExists)
                {
                    LoggingConfiguration config = LogManager.Configuration;

                    var eventLogTarget = new EventLogTarget
                    {
                        Layout = "${longdate}|${level:uppercase=true}|${logger:shortName=true}|${message}",
                        EventId = "${event-context:item=EventID}",
                        Log = LogName,
                        Source = Source,
                    };

                    config.AddTarget("event_log", eventLogTarget);

                    var eventLogRule = new LoggingRule("*", LogLevel.Info, eventLogTarget);

                    config.LoggingRules.Add(eventLogRule);

                    LogManager.ReconfigExistingLoggers();
                }
            }
            catch (Exception ex)
            {
                Logger log = LogManager.GetLogger(Source);
                log.ErrorException(Resources.InternalLog_ErrorInSettingUpEventLogger_Message, ex);
            }
        }
    }
}