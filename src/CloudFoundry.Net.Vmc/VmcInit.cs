namespace CloudFoundry.Net.Vmc
{
    using RestSharp;
    using System;
    using CloudFoundry.Net.Vmc.Properties;

    internal class VmcInit
    {
        private readonly Uri uri;
        private readonly string accessToken;

        public VmcInit(Uri argUri, string argAccessToken)
        {
            uri = argUri;
            accessToken = argAccessToken;

            if (null == uri)
            {
                // throw new ArgumentNullException(Resources.VmcInit_UriRequired_Message);
            }
        }

        public string Info (string accesstoken, string url)
        {
            if (url == null)
            {
                return ("Target URL has to be set");
            }
            else
            {
                var client = new RestClient();
                client.BaseUrl = url;
                var request = new RestRequest();
                request.Resource = "/info";
                if (accesstoken != null)
                    request.AddHeader("Authorization", accesstoken);
                return client.Execute(request).Content;
            }
        }
    }
}