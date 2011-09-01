using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudFoundry.Net.Dea.Providers.Interfaces;

namespace CloudFoundry.Net.Dea.Providers
{
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
