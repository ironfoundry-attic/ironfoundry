using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Warden.Service.InstallerCA
{
    using Microsoft.Deployment.WindowsInstaller;

    public interface ILogger
    {
        void LogError(string message);
        void Log(string format, params object[] args);
    }

    public class SessionLogger : ILogger
    {
        private readonly Session session;

        public SessionLogger(Session session)
        {
            this.session = session;
        }

        public void LogError(string message)
        {
            session.Log(message);
        }

        public void Log(string formatMessage, params object[] args)
        {
            session.Log(formatMessage, args);
        }
    }
}
