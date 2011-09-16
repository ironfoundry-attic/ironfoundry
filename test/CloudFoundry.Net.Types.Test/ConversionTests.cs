namespace CloudFoundry.Net.Types.Test
{
    using System;
    using System.Net;
    using Xunit;

    public class ConversionTests
    {
        [Fact]
        public void Can_Convert_Hello_Message()
        {
            Guid id = Guid.NewGuid();
            IPAddress addr;
            IPAddress.TryParse("10.0.0.1", out addr);

            string expected = @"{""id"":""FOO_1234"",""ip"":""10.0.0.1"",""port"":80,""version"":1.0}";

            var msg = new Hello(id, addr, 80, 1.0M);

            Assert.Equal(expected, msg.ToJson());
        }

        [Fact]
        public void Can_Convert_Vcap_Discover_Message() // TODO
        {
            Guid uuid = Guid.NewGuid();

            var msg = new VcapComponentDiscover("DEA", 1, uuid, "127.0.0.1:9999", uuid, DateTime.Now);

            var json = msg.ToJson();

            Console.WriteLine(json);

            // TODO assert
        }
    }
}