namespace IronFoundry.Bosh.Test
{
    using System.IO;
    using IronFoundry.Bosh.Blobstore;
    using Xunit;

    public class Blobstore
    {
        // [Fact(Skip="MANUAL")]
        [Fact]
        public void Can_Upload_File()
        {
            var options = new BlobstoreOptions("http://172.21.10.181:25250", "agent", "agent");
            var client = new SimpleBlobstoreClient(options);
            var localFile = new FileInfo(@"C:\proj\tmp\env.iso");
            string response = client.Create(localFile);
        }
    }
}