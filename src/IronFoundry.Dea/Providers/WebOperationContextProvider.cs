namespace IronFoundry.Dea.Providers
{
    using System;
    using System.IO;
    using System.Net;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

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