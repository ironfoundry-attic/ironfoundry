namespace IronFoundry.Warden.Utilities
{
    using System;
    using NLog;

    public static class LoggerExtensionMethods
    {
        public static void ErrorException(this Logger logger, Exception exception)
        {
            logger.LogException(LogLevel.Error, String.Empty, exception);
        }
    }
}
