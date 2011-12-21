namespace IronFoundry.Dea.JsonConverters
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    public class VcapGuidConverter : JsonConverter
    {
        private static readonly Type convertableType = typeof(Guid);

        public override bool CanConvert(Type objectType)
        {
            return (null != objectType && convertableType == objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (null == value)
            {
                throw new Exception("Attempt to convert null Guid value.");
            }
            else
            {
                string guidString = ((Guid)value).ToString("N");
                writer.WriteValue(guidString);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "Cannot convert null value to {0}.", objectType));
            }

            return Guid.ParseExact(reader.Value.ToString(), "N");
        }
    }
}