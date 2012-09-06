namespace IronFoundry.Bosh.Test
{
    using IronFoundry.Bosh.Agent.Handlers;
    using Xunit;

    public class JsonTests
    {
        [Fact]
        public void Serializes_Upload_Result()
        {
            object result = Test();
            var response = new HandlerResponse(result);
            string json = response.ToJson();
        }

        private object Test()
        {
            return new { result = new { sha1 = "foo", blobstore_id = "bar", compile_log_id = "baz" } };
        }
    }
}