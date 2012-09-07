namespace IronFoundry.Bosh.Test
{
    using System;
    using IronFoundry.Misc.Logging;

    public class NoopLogger : ILog
    {
        public void Flush() { }
        public void Debug(string message) { }
        public void Debug(string fmt, params object[] args) { }
        public void Error(string message) { }
        public void Error(string fmt, params object[] args) { }
        public void Error(Exception exception, string fmt, params object[] args) { }
        public void Error(Exception exception) { }
        public void Fatal(string message) { }
        public void Fatal(string fmt, params object[] args) { }
        public void Info(string message) { }
        public void Info(string fmt, params object[] args) { }
        public void Trace(string message) { }
        public void Trace(string fmt, params object[] args) { }
        public void Warn(string message) { }
        public void Warn(string fmt, params object[] args) { }
        public void EnableDebug() { }
        public void DisableDebug() { }
        public void AddFileTarget(string targetName, string logFilePath) { }
    }
}