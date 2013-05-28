namespace IronFoundry.Warden.Utilities
{
    using System;
    using System.IO;
    using IronFoundry.Warden.Configuration;
    using NLog;

    public class ServiceHelper
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly WardenConfig wardenConfig;

        public ServiceHelper()
        {
            this.wardenConfig = new WardenConfig();
        }

        public void OnStart()
        {
            try
            {
                string containerPath = wardenConfig.ContainerBasePath;
                Directory.CreateDirectory(containerPath);
            }
            catch (Exception ex)
            {
                log.ErrorException(String.Empty, ex);
                throw;
            }
        }
    }
}
