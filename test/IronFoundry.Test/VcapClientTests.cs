namespace IronFoundry.Test
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using IronFoundry.Vcap;
    using Xunit;

    public class VcapClientTests
    {
        private HttpListener httpListener;

        [Fact]
        public void Test_Using_Host_And_IPAddress()
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            string host = "localhost";

            var listenTask = StartWebServer();
            listenTask.ContinueWith(context =>
            {
                Assert.Equal(context.Result.Request.Headers["host"], host);
                context.Result.Response.OutputStream.Close();
            }).ContinueWith(context => StopWebServer());

            var uri = new Uri("http://" + host);
            var client = new VcapClient(uri, ip, 12345);
            client.GetInfo();
        }

        private Task<HttpListenerContext> StartWebServer()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:12345/");
            httpListener.Start();
            return httpListener.GetContextAsync();
        }

        private void StopWebServer()
        {
            httpListener.Stop();
        }
    }
}