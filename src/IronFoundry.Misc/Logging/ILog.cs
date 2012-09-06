namespace IronFoundry.Misc.Logging
{
    using System;

    public interface ILog
    {
        void Flush();

        void Debug(string message);
        void Debug(string fmt, params object[] args);

        void Error(string message);
        void Error(string fmt, params object[] args);
        void Error(Exception exception, string fmt, params object[] args);
        void Error(Exception exception);

        void Fatal(string message);
        void Fatal(string fmt, params object[] args);

        void Info(string message);
        void Info(string fmt, params object[] args);

        void Trace(string message);
        void Trace(string fmt, params object[] args);

        void Warn(string message);
        void Warn(string fmt, params object[] args);

        void EnableDebug();
        void DisableDebug();

        void AddFileTarget(string targetName, string logFilePath);
    }
}