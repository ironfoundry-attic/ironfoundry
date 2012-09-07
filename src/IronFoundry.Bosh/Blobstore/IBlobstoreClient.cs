namespace IronFoundry.Bosh.Blobstore
{

    public interface IBlobstoreClient
    {
        string Create(string localFilePath);
        void Delete(string blobstoreID);
        void Get(string blobstoreID, string localFilePath);
    }
}