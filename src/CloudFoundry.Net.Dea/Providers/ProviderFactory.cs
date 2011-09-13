namespace CloudFoundry.Net.Dea.Providers
{
    using CloudFoundry.Net.Dea.Providers.Interfaces;

    public class ProviderFactory : IProviderFactory
    {
        public IMessagingProvider CreateMessagingProvider(string argHost, ushort argPort)
        {
            return new NatsMessagingProvider(argHost, argPort);
        }

        public IWebServerAdministrationProvider CreateWebServerAdministrationProvider()
        {
            return new WebServerAdministrationProvider();
        }       
    }
}