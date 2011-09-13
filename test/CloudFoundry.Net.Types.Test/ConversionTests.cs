namespace CloudFoundry.Net.Types.Test
{
    using System;
    using System.Net;
    using Types.Messages;
    using Xunit;

    public class ConversionTests
    {
        [Fact]
        public void Can_Convert_Hello_Message()
        {
            string id = "FOO_1234";
            IPAddress addr;
            IPAddress.TryParse("10.0.0.1", out addr);

            string expected = @"{""id"":""FOO_1234"",""ip"":""10.0.0.1"",""port"":80,""version"":1.0}";

            var msg = new Hello
            {
                ID = id,
                IPAddress = addr,
                Port = 80,
                Version = 1.0M,
            };

            Assert.Equal(expected, msg.ToJson());
        }

        [Fact]
        public void Can_Convert_Vcap_Discover_Message() // TODO
        {
            Guid uuid = Guid.NewGuid();

            var msg = new VcapComponentDiscover
            {
                Type        = "DEA",
                Index       = 1,
                Uuid        = uuid.ToString(),
                Host        = "127.0.0.1:9999",
                Credentials = uuid.ToString(),
                Start       = DateTime.Now,
            };

            var json = msg.ToJson();

            Console.WriteLine(json);

            // TODO assert
        }
    }
}