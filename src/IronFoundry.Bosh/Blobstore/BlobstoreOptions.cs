namespace IronFoundry.Bosh.Blobstore
{
    using System;

    public class BlobstoreOptions
    {
        private static readonly char[] endpointTrimChars = new[] { '/' };
        private readonly Uri endpoint;
        private readonly string bucket;
        private readonly string user;
        private readonly string password;

        public BlobstoreOptions(Uri endpoint, string user, string password)
            : this(endpoint, null, user, password) { }

        public BlobstoreOptions(Uri endpoint, string bucket, string user, string password)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }
            this.endpoint = endpoint;
            this.bucket = bucket ?? "resources";
            this.user = user;
            this.password = password;
        }

        public string Endpoint { get { return endpoint.AbsoluteUri.TrimEnd(endpointTrimChars); } }
        public string Bucket { get { return bucket; } }
        public string User { get { return user; } }
        public string Password { get { return password; } }
    }
}