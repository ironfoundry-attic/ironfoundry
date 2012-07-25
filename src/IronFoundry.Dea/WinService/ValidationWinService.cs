namespace IronFoundry.Dea.WinService
{
    using System;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Misc.Configuration;

    [System.ComponentModel.DesignerCategory(@"Code")]
    public class ValidationWinService : IService
    {
        private readonly ILog log;
        private readonly IConfig config;

        public ValidationWinService(ILog log, IConfig config)
        {
            this.log = log;
            this.config = config;
        }

        public string ServiceName
        {
            get { return "IronFoundry.Dea.Service"; }
        }

        public ushort StartIndex
        {
            get { return 0; }
        }

        public StartServiceResult StartService(IntPtr serviceHandle)
        {
            var rv = new StartServiceResult();

            if (false == config.HasAppCmd)
            {
                rv.Success = false;
                string msg = Resources.ValidationWinService_AppCmdNotFound_Message;
                log.Error(msg);
                rv.Message = msg;
            }

            return rv;
        }

        public void StopService() { }
    }
}