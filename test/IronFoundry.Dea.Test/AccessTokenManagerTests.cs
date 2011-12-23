namespace IronFoundry.Test
{
    using System;
    using Vcap;
    using Xunit;

    public class AccessTokenManagerTests
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

            var tokenManager = new VcapCredentialManager(json, false);

            tokenManager.SetTarget(uri1.AbsoluteUri);
            var token1 = tokenManager.CurrentToken;
            Assert.Equal(tokenStr1, token1);

            tokenManager.SetTarget(uri2.AbsoluteUri);
            var token2 = tokenManager.CurrentToken;
            Assert.Equal(tokenStr2, token2);
        }
    }
}