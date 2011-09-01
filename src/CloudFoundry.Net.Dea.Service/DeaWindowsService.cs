using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using NLog;

namespace CloudFoundry.Net.Dea.Service
{
    [System.ComponentModel.DesignerCategory(@"Code")]
    partial class DeaWindowsService : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DeaWindowsService()
        {
            CanPauseAndContinue = false;

            InitializeEventLog();
        }

        private void InitializeEventLog()
        {
            try
            {
                AutoLog = false;
                EventLogger.Info("Init logging.");
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Unable to setup event log.", ex);
                AutoLog = true;
            }
        }

        public void StartService()
        {
            OnStart(null);
        }

        public void StopService()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
