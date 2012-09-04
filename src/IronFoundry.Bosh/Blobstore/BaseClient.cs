namespace IronFoundry.Bosh.Blobstore
{
    using System;
    using System.IO;

    public abstract class BaseClient
    {
        protected readonly BlobstoreOptions options;

        public BaseClient(BlobstoreOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            this.options = options;
        }

        public abstract string Create(FileInfo localFile);
        public abstract void Get(string blobstoreID, FileInfo localFile);
        public abstract void Delete(string blobstoreID);
    }
}