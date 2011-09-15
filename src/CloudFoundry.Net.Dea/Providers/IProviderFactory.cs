namespace CloudFoundry.Net.Dea.Providers
{
    public interface IProviderFactory
    {
        IMessagingProvider CreateMessagingProvider(string argHost, ushort argPort);
        IWebServerAdministrationProvider CreateWebServerAdministrationProvider();
    }
}