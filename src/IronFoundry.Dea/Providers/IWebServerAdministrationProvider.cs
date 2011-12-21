namespace IronFoundry.Dea.Providers
{

    public enum ApplicationInstanceStatus
    {
        Started,
        Starting,
        Stopped,
        Stopping,
        Unknown
    }

    public class WebServerAdministrationBinding
    {
        public string Host;
        public ushort Port;
    }

    public interface IWebServerAdministrationProvider
    {
        WebServerAdministrationBinding InstallWebApp(string localDirectory, string applicationInstanceName);
        void UninstallWebApp(string applicationInstanceName);
        void Start(string applicationInstanceName);
        void Stop(string applicationInstanceName);
        void Restart(string applicationInstanceName);
        ApplicationInstanceStatus GetStatus(string applicationInstanceName);
        bool DoesApplicationExist(string applicationInstanceName);
    }
}