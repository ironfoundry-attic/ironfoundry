namespace IronFoundry.Utilities
{
    using Newtonsoft.Json;
    using RestSharp;
    using RestSharp.Deserializers;

    public class NewtonsoftJsonDeserializer : IDeserializer
    {
        public const string JsonContentType = "application/json"; 

        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string ContentType
        {
            get { return JsonContentType; }
        }
    }
}