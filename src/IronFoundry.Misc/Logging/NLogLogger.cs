namespace IronFoundry.Misc.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    public class NLogLogger : ILog
    {
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

        public NLogLogger(string name)
        {
            logger = LogManager.GetLogger(name);
        }

        public void Flush()
        {
            LogManager.Flush();
        }

        public void EnableDebug()
        {
            modifyFileLoggingRules(lr => lr.EnableLoggingForLevel(LogLevel.Debug));
        }

        public void DisableDebug()
        {
            modifyFileLoggingRules(lr => lr.DisableLoggingForLevel(LogLevel.Debug));
        }

        public void Debug(string message)
        {
            if (logger.IsDebugEnabled)
            {
                log(LogLevel.Debug, message);
            }
        }

        public void Debug(string fmt, params object[] args)
        {
            if (logger.IsDebugEnabled)
            {
                log(LogLevel.Debug, fmt, args);
            }
        }

        public void Error(string message)
        {
            if (logger.IsErrorEnabled)
            {
                log(LogLevel.Error, message);
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

        public void Fatal(string message)
        {
            if (logger.IsFatalEnabled)
            {
                log(LogLevel.Fatal, message);
            }
        }

        public void Fatal(string fmt, params object[] args)
        {
            if (logger.IsFatalEnabled)
            {
                log(LogLevel.Fatal, fmt, args);
            }
        }

        public void Info(string message)
        {
            if (logger.IsInfoEnabled)
            {
                log(LogLevel.Info, message);
            }
        }

        public void Info(string fmt, params object[] args)
        {
            if (logger.IsInfoEnabled)
            {
                log(LogLevel.Info, fmt, args);
            }
        }

        public void Trace(string message)
        {
            if (logger.IsTraceEnabled)
            {
                log(LogLevel.Trace, message);
            }
        }

        public void Trace(string fmt, params object[] args)
        {
            if (logger.IsTraceEnabled)
            {
                log(LogLevel.Trace, fmt, args);
            }
        }

        public void Warn(string message)
        {
            if (logger.IsWarnEnabled)
            {
                log(LogLevel.Warn, message);
            }
        }

        public void Warn(string fmt, params object[] args)
        {
            if (logger.IsWarnEnabled)
            {
                log(LogLevel.Warn, fmt, args);
            }
        }

        public void AddFileTarget(string targetName, string logFilePath)
        {
            LoggingConfiguration config = LogManager.Configuration;
            var newFileTarget = new FileTarget {
                Name = targetName,
                FileName = logFilePath,
                Layout = "${longdate}|${level:uppercase=true}|${logger:shortName=true}|${message}${onexception:|${exception:format=Message}}",
                DeleteOldFileOnStartup = true,
                CreateDirs = true,
                AutoFlush = true,
                EnableFileDelete = true,
            };
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, newFileTarget));
            LogManager.ReconfigExistingLoggers();
        }

        private void log(LogLevel level, string message)
        {
            logit(new LogEventInfo(level, logger.Name, message));
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

        private static void modifyFileLoggingRules(Action<LoggingRule> fileLoggingRuleAction)
        {
            LoggingConfiguration config = LogManager.Configuration;
            Target fileTarget = config.FindTargetByName("file");
            foreach (LoggingRule fileLoggingRule in config.LoggingRules.Where(lr => lr.Targets.Contains(fileTarget)))
            {
                fileLoggingRuleAction(fileLoggingRule);
            }
            LogManager.ReconfigExistingLoggers();
        }
    }
}