namespace CloudFoundry.Net.Dea.Providers.Interfaces
{
    public interface IProviderFactory
    {
        IMessagingProvider CreateMessagingProvider(string argHost, ushort argPort);
        IWebServerAdministrationProvider CreateWebServerAdministrationProvider();
    }
}