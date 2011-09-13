namespace CloudFoundry.Net.Types
{
    using Newtonsoft.Json;

    public abstract class JsonBase
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return ToJson();
        }

        public static T FromJson<T>(string argJson)
        {
            return JsonConvert.DeserializeObject<T>(argJson);
        }
    }
}