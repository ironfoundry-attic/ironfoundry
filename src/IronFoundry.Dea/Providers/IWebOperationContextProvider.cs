namespace IronFoundry.Dea.Providers
{
    using System;
    using System.IO;
    using System.Net;
    using System.ServiceModel.Channels;

    public interface IWebOperationContextProvider
    {        
        Uri GetRequestUri();
        Message CreateStreamResponse(Stream stream, string contentType);
        void SetOutgoingResponse(HttpStatusCode code, string description);
    }
}
