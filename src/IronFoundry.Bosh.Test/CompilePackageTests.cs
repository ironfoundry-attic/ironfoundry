namespace IronFoundry.Bosh.Test
{
    using System;
    using System.IO;
    using IronFoundry.Bosh.Agent.Handlers;
    using IronFoundry.Bosh.Blobstore;
    using Moq;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class CompilePackageTests
    {
        [Fact]
        public void Processes_Compile_Package_Message()
        {
            string createBlobstoreID = Guid.NewGuid().ToString();

            string compileJson = @"{""method"":""compile_package"",""arguments"":[""4bee4374-7028-4612-bf9a-68173eb04a4c"",""fbdc0b775f2aa022da932c87d6a4924a9482b1d4"",""mysql"",""0.1-dev.1"",{}],""reply_to"":""director.02038a67-e537-4d8e-98c0-fbd9422883b4.92317dd3-80e1-4950-ad66-621a73a1a7a2""}";
            var jobject = JObject.Parse(compileJson);
            var config = new TestBoshConfig();

            var blobstoreClient = new Mock<IBlobstoreClient>();
            blobstoreClient.Setup(x => x.Get(It.Is<string>(v => v == "4bee4374-7028-4612-bf9a-68173eb04a4c"), It.IsAny<string>()))
                .Callback<string, string>((blobstoreID, file) =>
                {
                    File.Copy("iis-setup.tgz", file);
                });
            blobstoreClient.Setup(x => x.Create(It.IsAny<string>())).Returns(createBlobstoreID);

            var blobstoreClientFactory = new Mock<IBlobstoreClientFactory>();
            blobstoreClientFactory.Setup(x => x.Create()).Returns(blobstoreClient.Object);

            var handler = new CompilePackage(config, new NoopLogger(), blobstoreClientFactory.Object);
            HandlerResponse result = handler.Handle(jobject);
            Assert.NotNull(result.Value);
            string resultJson = result.ToJson();
            Assert.Contains(createBlobstoreID, resultJson);
        }
    }
}