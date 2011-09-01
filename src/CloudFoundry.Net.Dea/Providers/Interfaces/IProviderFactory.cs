using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Dea.Providers.Interfaces
{
    public interface IProviderFactory
    {
        IMessagingProvider CreateMessagingProvider(string host, int port);
        IWebServerAdministrationProvider CreateWebServerAdministrationProvider();
    }
}
