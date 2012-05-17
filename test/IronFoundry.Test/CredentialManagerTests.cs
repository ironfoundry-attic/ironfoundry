namespace IronFoundry.Test
{
    using System;
    using System.Net;
    using Vcap;
    using Xunit;

    public class CredentialManagerTests
    {
        [Fact]
        public void Can_Parse_AccessToken_JSON()
        {
            var uri1 = new Uri(@"http://api.vcap.me");
            var tokenStr1 = Guid.NewGuid().ToString("N");

            var uri2 = new Uri(@"http://api_two.vcap.me");
            var tokenStr2 = Guid.NewGuid().ToString("N");

            var json = String.Format("{{\"{0}\":\"{1}\",\"{2}\":\"{3}\"}}",
                                        uri1.AbsoluteUri, tokenStr1, uri2.AbsoluteUri, tokenStr2);

            var credentialManager = new VcapCredentialManager(json, false);

            credentialManager.SetTarget(uri1.AbsoluteUri);
            var token1 = credentialManager.CurrentToken;
            Assert.Equal(tokenStr1, token1);

            credentialManager.SetTarget(uri2.AbsoluteUri);
            var token2 = credentialManager.CurrentToken;
            Assert.Equal(tokenStr2, token2);
        }

        [Fact]
        public void Can_Create_With_Host_And_IP()
        {
            string ipStr = "10.0.0.1";
            Uri uri = new Uri("http://api.vcap-test.me");

            IPAddress ip;
            IPAddress.TryParse(ipStr, out ip);

            var credentialManager = new VcapCredentialManager(uri, ip);
            Assert.Equal(uri, credentialManager.CurrentTarget);
            Assert.NotNull(credentialManager.CurrentTargetIP);
            Assert.Equal(ip, credentialManager.CurrentTargetIP);

            string newTarget = "http://api.foo.com";
            var newTargetUri = new Uri(newTarget);
            credentialManager.SetTarget("http://api.foo.com");
            Assert.Equal(newTargetUri, credentialManager.CurrentTarget);
            Assert.Null(credentialManager.CurrentTargetIP);
        }
    }
}