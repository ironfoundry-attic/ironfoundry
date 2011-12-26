namespace IronFoundry.Dea.WinService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Properties;
    using IronFoundry.Dea.Services;
    using IronFoundry.Dea.WcfInfrastructure;

    public class FilesWinService : WcfWinService
    {
        private readonly IConfig config;
        private readonly IFirewallService firewallService;

        public FilesWinService(ILog log, IConfig config, IFirewallService firewallService) : base(log, true)
        {
            this.config = config;
            this.firewallService = firewallService;

            Uri baseAddress = config.WCFFilesServiceUri;

            var webHttpBinding = new WebHttpBinding();
            webHttpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            webHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var serviceHost =  new IocServiceHost(typeof(FilesService), baseAddress);               
            base.serviceHost = serviceHost;

            ServiceEndpoint endpoint = serviceHost.AddServiceEndpoint(typeof(IFilesService), webHttpBinding, baseAddress);
            endpoint.Behaviors.Add(new WebHttpBehavior());
            serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new FilesServiceValidator(config);
            serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 
        }

        public override ushort StartIndex
        {
            get { return 0; }
        }

        public override StartServiceResult StartService(IntPtr ignored)
        {
            firewallService.Open(config.FilesServicePort, Resources.FilesWinService_ServiceName);
            return base.StartService(ignored);
        }

        public override void StopService()
        {
            firewallService.Close(Resources.FilesWinService_ServiceName);
            base.StopService();
        }
    }
}