namespace IronFoundry.Dea.WinService
{
    using System;
    using System.ServiceModel;
    using IronFoundry.Dea.Config;
    using IronFoundry.Dea.Logging;
    using IronFoundry.Dea.Services;
    using IronFoundry.Dea.WcfInfrastructure;

    public class FilesWinService : WcfWinService
    {
        public FilesWinService(ILog log, IConfig config) : base(log, true)
        {
            var baseAddress = new Uri("http://localhost:" + config.FilesServicePort);

            var httpBinding = new WebHttpBinding(); // TODO: message sizes
            httpBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var serviceHost =  new IocSingletonWebServiceHost(typeof(FilesService), baseAddress);
            base.serviceHost = serviceHost;

            serviceHost.AddServiceEndpoint(typeof(IFilesService), httpBinding, baseAddress);

            /*
             * TODO
            serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new UserCustomAuthentication(this.username, this.password);
            serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom; 
            ((FileServerService)this.host.SingletonInstance).Initialize(this.serverPhysicalPath, this.serverVirtualPath);
             */
        }

        public override ushort StartIndex
        {
            get { return 0; }
        }
    }
}