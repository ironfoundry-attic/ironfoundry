namespace IronFoundry.Bosh.Blobstore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using IronFoundry.Bosh.Properties;
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

        public override string Create(FileInfo localFile)
        {
            if (false == localFile.Exists)
            {
                throw new ArgumentException(Resources.BlobstoreClient_LocalFileDoesNotExist_Fmt, localFile.FullName);
            }
            var request = new RestRequest(GetUrl(), Method.POST);
            request.AddFile("content", localFile.FullName);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new BlobstoreException(Resources.BlobstoreClient_CouldNotCreateObject_Fmt, response.StatusCode, response.Content);
            }
            return response.Content;
        }

        public override void Get(string blobstoreID, FileInfo localFile)
        {
            var request = new RestRequest(GetUrl(blobstoreID), Method.GET);
            IRestResponse response;
            using (var fs = File.Open(localFile.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                request.ResponseWriter = (responseStream) => responseStream.CopyTo(fs);
                response = client.Execute(request);
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new BlobstoreException(Resources.BlobstoreClient_CouldNotFetchObject_Fmt, response.StatusCode, response.Content);
            }
        }

        public override void Delete(string blobstoreID)
        {
            var request = new RestRequest(GetUrl(blobstoreID), Method.DELETE);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new BlobstoreException(Resources.BlobstoreClient_CouldNotDeleteObject_Fmt, response.StatusCode, response.Content);
            }
        }

        private string GetUrl(string id = null)
        {
            var s = new[] { options.Endpoint, options.Bucket, id };
            return String.Join("/", s.Compact());
        }
    }
}