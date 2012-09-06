namespace IronFoundry.Bosh.Test
{
    using System;
    using IronFoundry.Bosh.Blobstore;
    using Xunit;

    public class Blobstore
    {
        [Fact(Skip="MANUAL")]
        public void Can_Upload_File()
        {
            var options = new BlobstoreOptions(new Uri("http://172.21.10.181:25250"), "agent", "agent");
            var client = new SimpleBlobstoreClient(options);
            string response = client.Create(@"C:\proj\tmp\env.iso");
        }
    }
}