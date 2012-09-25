namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json;

    public class UploadResult
    {
        private readonly string sha1;
        private readonly string blobstoreID;
        private readonly string compileLogID;

        public UploadResult(string sha1, string blobstoreID, string compileLogID)
        {
            this.sha1 = sha1;
            this.blobstoreID = blobstoreID;
            this.compileLogID = compileLogID;
        }

        [JsonProperty(PropertyName = "sha1")]
        public string SHA1 { get { return sha1; } }

        [JsonProperty(PropertyName = "blobstore_id")]
        public string BlobstoreID { get { return blobstoreID; } }

        [JsonProperty(PropertyName = "compile_log_id")]
        public string CompileLogID { get { return compileLogID; } }
    }
}