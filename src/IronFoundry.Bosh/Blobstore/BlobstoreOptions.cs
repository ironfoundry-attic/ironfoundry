namespace IronFoundry.Bosh.Blobstore
{
    using System;

    public class BlobstoreOptions
    {
        private readonly string endpoint;
        private readonly string bucket;
        private readonly string user;
        private readonly string password;

        public BlobstoreOptions(string endpoint, string user, string password)
            : this(endpoint, null, user, password) { }

        public BlobstoreOptions(string endpoint, string bucket, string user, string password)
        {
            if (String.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentNullException("endpoint");
            }
            this.endpoint = endpoint;
            this.bucket = bucket ?? "resources";
            this.user = user;
            this.password = password;
        }

        public string Endpoint { get { return endpoint; } }
        public string Bucket { get { return bucket; } }
        public string User { get { return user; } }
        public string Password { get { return password; } }
    }
}