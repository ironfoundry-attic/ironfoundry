namespace IronFoundry.Bosh.Agent.Handlers
{
    using Newtonsoft.Json;

    public class UploadResult
    {
        private string sha1;
        private string blobstoreID;
        private string compileLogID;

        [JsonProperty(PropertyName = "sha1")]
        public string SHA1 { get { return sha1; } }
        [JsonProperty(PropertyName = "blobstore_id")]
        public string BlobstoreID { get { return blobstoreID; } }
        [JsonProperty(PropertyName = "compile_log_id")]
        public string CompileLogID { get { return compileLogID; } }

        public UploadResult(string sha1, string blobstoreID, string compileLogID)
        {
            this.sha1 = sha1;
            this.blobstoreID = blobstoreID;
            this.compileLogID = compileLogID;
        }
    }
}