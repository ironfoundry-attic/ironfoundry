namespace IronFoundry.Bosh.Blobstore
{
    public interface IBlobstoreClientFactory
    {
        BlobstoreClient Create();
    }
}