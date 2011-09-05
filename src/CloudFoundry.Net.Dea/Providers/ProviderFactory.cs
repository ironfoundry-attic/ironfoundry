namespace CloudFoundry.Net.Dea.Providers
{
    using CloudFoundry.Net.Dea.Providers.Interfaces;

    public class ProviderFactory : IProviderFactory
    {
        public IMessagingProvider CreateMessagingProvider(string host, int port)
        {
            return new NatsMessagingProvider(host, port);
        }

        public IWebServerAdministrationProvider CreateWebServerAdministrationProvider()
        {
            return new WebServerAdministrationProvider();
        }       
    }
}