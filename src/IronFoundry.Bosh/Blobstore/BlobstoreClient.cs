namespace IronFoundry.Bosh.Blobstore
{
    using System;

    public abstract class BlobstoreClient
    {
        protected readonly BlobstoreOptions options;

        public BlobstoreClient(BlobstoreOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            this.options = options;
        }

        public abstract string Create(string localFilePath);
        public abstract void Get(string blobstoreID, string localFilePath);
        public abstract void Delete(string blobstoreID);
    }
}