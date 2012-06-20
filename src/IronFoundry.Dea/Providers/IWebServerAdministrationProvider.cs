namespace IronFoundry.Dea.Providers
{
    using System.Collections.Generic;
    using IronFoundry.Dea.Types;

    public class WebServerAdministrationBinding
    {
        public string Host;
        public ushort Port;
    }

    public interface IWebServerAdministrationProvider
    {
        WebServerAdministrationBinding InstallWebApp(string localDirectory, string applicationInstanceName);
        void UninstallWebApp(Instance applicationInstance);
        ApplicationInstanceStatus GetApplicationStatus(Instance applicationInstance);
        IDictionary<string, IList<int>> GetIIsWorkerProcesses();
    }
}