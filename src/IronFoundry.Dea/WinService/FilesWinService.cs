using System.ServiceModel.Description;

namespace IronFoundry.Dea.WinService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Services;
    using IronFoundry.Dea.WcfInfrastructure;

    public class FilesWinService : WcfWinService
    {
        public FilesWinService(ILog log, IConfig config) : base(log, true)
        {
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
    }
}