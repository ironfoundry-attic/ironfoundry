namespace IronFoundry.Test
{
    using IronFoundry.Dea.Types;
    using Newtonsoft.Json;
    using Xunit;

    public class JsonTests
    {
        string json = @"{""droplet"":2,""name"":""testwebapp"",""uris"":[""testwebapp.vcap.me""],""runtime"":""aspdotnet40"",""framework"":""aspdotnet"",""prod"":false,""sha1"":""dd71b9954568f6be4e8c5d90e5476e98034f3235"",""executableFile"":""/var/vcap/shared/droplets/droplet_2"",""executableUri"":""http://192.168.171.129:9022/staged_droplets/2/dd71b9954568f6be4e8c5d90e5476e98034f3235"",""version"":""dd71b9954568f6be4e8c5d90e5476e98034f3235-1"",""services"":[{""name"":""mssb-4e356"",""type"":""generic"",""label"":""mssb-1.0"",""vendor"":""mssb"",""version"":""1.0"",""tags"":[""mssb"",""mssb-1.0-beta"",""message-queue"",""Microsoft Service Bus""],""plan"":""free"",""plan_option"":null,""credentials"":{""name"":""nsc2b234e121834484846e4a5f8f2b25ab"",""hostname"":""192.168.171.131"",""host"":""192.168.171.131"",""username"":""u1iMNcvWCRwBg"",""password"":""pTlW5ZAYdjksf"",""sb_oauth_https"":""https://192.168.171.131:4446/nsc2b234e121834484846e4a5f8f2b25ab/$STS/OAuth/"",""sb_oauth"":""sb://192.168.171.131:4446/nsc2b234e121834484846e4a5f8f2b25ab/"",""sb_runtime_address"":""sb://192.168.171.131:9354/nsc2b234e121834484846e4a5f8f2b25ab/""}}],""limits"":{""mem"":64,""disk"":2048,""fds"":256},""env"":[],""users"":[""bob.abooey@foo.com""],""debug"":null,""console"":false,""index"":0}";

        [Fact]
        public void Can_Deserialize_To_Droplet()
        {
            Droplet droplet = JsonConvert.DeserializeObject<Droplet>(json);
            Assert.NotNull(droplet);
        }
    }
}
