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

            AccessToken token1 = tokenManager.GetFor(uri1.AbsoluteUri);
            Assert.NotNull(token1);
            Assert.Equal(tokenStr1, token1.Token);

            AccessToken token2 = tokenManager.GetFor(uri2.AbsoluteUri);
            Assert.NotNull(token2);
            Assert.Equal(tokenStr2, token2.Token);
        }
    }
}