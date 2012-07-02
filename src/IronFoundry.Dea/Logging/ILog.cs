namespace IronFoundry.Dea.Logging
{
    using System;

    public interface ILog
    {
        void Debug(string fmt, params object[] args);

        void Error(string fmt, params object[] args);
        void Error(Exception exception, string fmt, params object[] args);
        void Error(Exception exception);

        void Fatal(string fmt, params object[] args);
        void Info(string fmt, params object[] args);
        void Trace(string fmt, params object[] args);
        void Warn(string fmt, params object[] args);

        void EnableDebug();
        void DisableDebug();
    }
}