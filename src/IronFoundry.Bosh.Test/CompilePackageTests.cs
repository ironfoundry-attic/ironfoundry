namespace IronFoundry.Bosh.Test
{
    using IronFoundry.Bosh.Agent.Handlers;
    using IronFoundry.Bosh.Blobstore;
    using IronFoundry.Bosh.Configuration;
    using Moq;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class CompilePackageTests
    {
        [Fact]
        public void Processes_Compile_Package_Message()
        {
            string compileJson = @"{""method"":""compile_package"",""arguments"":[""4bee4374-7028-4612-bf9a-68173eb04a4c"",""fbdc0b775f2aa022da932c87d6a4924a9482b1d4"",""mysql"",""0.1-dev.1"",{}],""reply_to"":""director.02038a67-e537-4d8e-98c0-fbd9422883b4.92317dd3-80e1-4950-ad66-621a73a1a7a2""}";
            var jobject = JObject.Parse(compileJson);

            var config = new Mock<IBoshConfig>();
            config.Setup(x => x.BaseDir).Returns(BoshConfig.DefaultBaseDir);

            var blobstoreClientFactory = new Mock<IBlobstoreClientFactory>();
            var handler = new CompilePackage(config.Object, new NoopLogger(), blobstoreClientFactory.Object);
            var result = handler.Handle(jobject);
            Assert.NotNull(result.Value);
        }
    }
}