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
    public class WebOperationContextProvider : IWebOperationContextProvider
    {        
        public Uri GetRequestUri()
        {
            return WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;
        }

        public Message CreateStreamResponse(Stream stream, string contentType)
        {
            return WebOperationContext.Current.CreateStreamResponse(stream, contentType);
        }

        public void SetOutgoingResponse(HttpStatusCode code, string description)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = code;
            WebOperationContext.Current.OutgoingResponse.StatusDescription = description;
        }
    }
}
