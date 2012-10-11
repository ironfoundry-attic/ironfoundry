namespace IronFoundry.Dea.WinService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using IronFoundry.Dea.Configuration;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Services;
    using IronFoundry.Dea.WcfInfrastructure;
    using IronFoundry.Misc.Logging;
    using IronFoundry.Misc.WinService;

    /// <summary>
    /// TODO: duplicated code with MonitoringWinService
    /// </summary>
    public class FilesWinService : WcfWinService
    {
        private readonly IDeaConfig config;
        private readonly IFirewallService firewallService;

        public FilesWinService(ILog log, IDeaConfig config, IFirewallService firewallService) : base(log, true)
        {
            this.config = config;
            this.firewallService = firewallService;

            Uri baseAddress = config.FilesServiceUri;

            var webHttpBinding = new WebHttpBinding();
            webHttpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            webHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var serviceHost =  new IocServiceHost(typeof(FilesService), baseAddress);               
            base.serviceHost = serviceHost;

            ServiceEndpoint endpoint = serviceHost.AddServiceEndpoint(typeof(IFilesService), webHttpBinding, baseAddress);
            endpoint.Behaviors.Add(new WebHttpBehavior());
            ServiceCredential filesCredentials = config.FilesCredentials;
#if DEBUG
            log.Debug("FilesWinService baseAddress: {0} credentials: {1}:{2}",
                baseAddress, filesCredentials.Username, filesCredentials.Password);
#endif
            serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUserNamePasswordValidator(filesCredentials);
            serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 
        }

        public override ushort StartIndex
        {
            get { return 1; }
        }

        public override StartServiceResult StartService(IntPtr ignored, string[] args)
        {
            firewallService.Open(config.FilesServicePort, Resources.FilesWinService_ServiceName);
            return base.StartService(ignored, args);
        }

        public override void StopService()
        {
            firewallService.Close(Resources.FilesWinService_ServiceName);
            base.StopService();
        }
    }
}