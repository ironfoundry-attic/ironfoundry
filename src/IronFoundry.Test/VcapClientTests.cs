namespace IronFoundry.Test
{
    using System;
    using System.Net;
    using IronFoundry.Utilities;
    using IronFoundry.Vcap;
    using RestSharp;
    using Xunit;

    public class VcapClientTests
    {
        [Fact]
        public void Test_Using_Host_And_IPAddress()
        {
            string ipStr = "10.0.0.1";
            string host = "api.vcap.me";

            var uri = new Uri("http://" + host);
            IPAddress ip;
            IPAddress.TryParse(ipStr, out ip);
            var client = new VcapClient(uri, ip);
            VcapRequest infoRequest = client.GetRequestForTesting();

            RestClient restClient = infoRequest.Client;
            RestRequest restRequest = infoRequest.Request;

            Assert.Equal("http://" + ipStr, restClient.BaseUrl);
            Assert.Equal(host, infoRequest.RequestHostHeader);

            Assert.NotNull(restRequest.JsonSerializer);
            Assert.IsType<NewtonsoftJsonSerializer>(restRequest.JsonSerializer);
        }

        [Fact(Skip="MANUAL")]
        public void Test_Using_Host_And_IPAddress_Against_Local()
        {
            string ipStr = "172.21.114.11";
            string host = "api.vcap.me";

            var uri = new Uri("http://" + host);
            IPAddress ip;
            IPAddress.TryParse(ipStr, out ip);
            var client = new VcapClient(uri, ip);
            client.Login("user@foo.com", "Password");
        }
    }
}