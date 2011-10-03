namespace CloudFoundry.Net.Types.Test
{
    using System;
    using CloudFoundry.Net.Vmc;
    using Xunit;

    public class AccessTokenManagerTests
    {
        [Fact]
        public void Can_Parse_AccessToken_JSON()
        {
            Uri uri1 = new Uri(@"http://api.vcap.me");
            string tokenStr1 = Guid.NewGuid().ToString("N");

            Uri uri2 = new Uri(@"http://api_two.vcap.me");
            string tokenStr2 = Guid.NewGuid().ToString("N");

            string json = String.Format("{{\"{0}\":\"{1}\",\"{2}\":\"{3}\"}}",
                uri1.AbsoluteUri, tokenStr1, uri2.AbsoluteUri, tokenStr2);

            var tokenManager = new VcapCredentialManager(json, false);

            tokenManager.SetTarget(uri1.AbsoluteUri);
            string token1 = tokenManager.CurrentToken;
            Assert.Equal(tokenStr1, token1);

            tokenManager.SetTarget(uri2.AbsoluteUri);
            string token2 = tokenManager.CurrentToken;
            Assert.Equal(tokenStr2, token2);
        }
    }
}