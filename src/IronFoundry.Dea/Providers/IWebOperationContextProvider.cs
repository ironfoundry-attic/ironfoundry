using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;

namespace IronFoundry.Dea.Providers
{
    public interface IWebOperationContextProvider
    {        
        Uri GetRequestUri();
        Message CreateStreamResponse(Stream stream, string contentType);
        void SetOutgoingResponse(HttpStatusCode code, string description);
    }
}
