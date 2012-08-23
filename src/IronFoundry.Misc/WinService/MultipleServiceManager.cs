namespace IronFoundry.Misc.WinService
{
    using System;
    using System.Linq;
    using System.ServiceProcess;
    using IronFoundry.Misc.Logging;

    public interface IMultipleServiceManager
    {
        void StartServiceManager();
        void StopServiceManager();
    }

    [System.ComponentModel.DesignerCategory("Code")]
    public class MultipleServiceManager : ServiceBase, IMultipleServiceManager
    {
        private readonly ILog log;
        private readonly IService[] services;

        /*
         * TODO BOSH AGENT
         * Create task factory and start each service on own thread?
         * Must have way for starting service to throw exception and cause start to fail for all services.
         */
        public MultipleServiceManager(ILog log, IService[] services)
        {
            this.log = log;
            this.services = services.OrderBy(s => s.StartIndex).ToArray();
            ServiceName = "IronFoundryDEA"; // NB: must match installer Product.wxs TODO BOSH AGENT
            AutoLog = true;
        }

        public void StartServiceManager()
        {
            log.Debug("StartServiceManager()");
            this.OnStart(null);
        }

        public void StopServiceManager()
        {
            log.Debug("StopServiceManager()");
            this.OnStop();
        }

        protected override void OnCustomCommand(int command)
        {
            switch (command)
            {
                case (int)CustomCommands.File_Logging_Enable_Debug :
                    log.EnableDebug();
                    break;
                case (int)CustomCommands.File_Logging_Disable_Debug :
                    log.DisableDebug();
                    break;
                default :
                    log.Debug("Unrecognized custom command: {0}", command);
                    break;
            }
        }

        protected override void OnStart(string[] args)
        {
            bool errorExit = false;
            try
            {
                foreach (IService s in services)
                {
                    StartServiceResult result = s.StartService(base.ServiceHandle);
                    if (false == result.Success)
                    {
                        errorExit = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception in OnStart");
                errorExit = true;
            }
            finally
            {
                if (errorExit)
                {
                    ServiceStatusUtil.ErrorExit(log, base.ServiceHandle, ServiceStatusUtil.ERROR_SERVICE_SPECIFIC_ERROR);
                }
            }

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            try
            {
                foreach (IService s in services)
                {
                    s.StopService();

                    IDisposable disposableService = s as IDisposable;
                    if (null != disposableService)
                    {
                        disposableService.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception in OnStop: {0}", ex.Message);
            }
            base.OnStop();
        }
    }
}