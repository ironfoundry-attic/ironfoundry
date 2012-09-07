namespace IronFoundry.Bosh.Blobstore
{
    public interface IBlobstoreClientFactory
    {
        IBlobstoreClient Create();
    }
}