namespace CloudFoundry.Net.JsonConverters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;

    public class VcapGuidArrayConverter : JsonConverter
    {
        private static readonly Type convertableType = typeof(Guid[]);

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
                var guids = (Guid[])value;
                writer.WriteStartArray();
                foreach (Guid g in guids)
                {
                    writer.WriteValue(((Guid)value).ToString("N"));
                }
                writer.WriteEndArray();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var rv = new List<Guid>();

            if (reader.TokenType == JsonToken.Null)
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "Cannot convert null value to {0}.", objectType));
            }

            if (reader.TokenType == JsonToken.StartArray)
                reader.Read();

            do
            {
                rv.Add(Guid.ParseExact(reader.Value.ToString(), "N"));
                reader.Read();
            } while (reader.TokenType != JsonToken.EndArray);

            return rv.ToArrayOrNull();
        }
    }
}