namespace IronFoundry.Bosh.Blobstore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using RestSharp;

    public class SimpleBlobstoreClient : BaseClient
    {
        private readonly RestClient client;

        public SimpleBlobstoreClient(BlobstoreOptions options) : base(options)
        {
            this.client = new RestClient();

            if (false == String.IsNullOrWhiteSpace(options.User) &&
                false == String.IsNullOrWhiteSpace(options.Password))
            {
                string authInfo = String.Format("{0}:{1}", options.User, options.Password);
                string authHeader = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
                client.AddDefaultHeader("Authorization", authHeader);
            }

        }

        public override void CreateFile(FileStream fs)
        {
            if (false == fs.CanRead)
            {
                throw new ArgumentException("fs must be readable");
            }
            var request = new RestRequest(GetUrl(), Method.POST);
            request.AddFile("content", (str) => fs.CopyTo(str), "content");
            IRestResponse response = client.Execute(request);
        }

        private string GetUrl(string id = null)
        {
            var s = new[] { options.Endpoint, options.Bucket, id };
            return String.Join("/", s.Compact());
        }
    }
}