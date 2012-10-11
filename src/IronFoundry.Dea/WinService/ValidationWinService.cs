namespace IronFoundry.Dea.WinService
{
    using System;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Configuration;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.WinService;

    [System.ComponentModel.DesignerCategory(@"Code")]
    public class ValidationWinService : IService
    {
        private readonly ILog log;
        private readonly IDeaConfig config;

        public ValidationWinService(ILog log, IDeaConfig config)
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

        public StartServiceResult StartService(IntPtr serviceHandle, string[] args)
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