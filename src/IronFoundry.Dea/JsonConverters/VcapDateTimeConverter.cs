namespace IronFoundry.Dea.JsonConverters
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    public class VcapDateTimeConverter : JsonConverter
    {
        private const string dateFormat = "yyyy-MM-dd HH:mm:ss zzz";
        private static readonly Type convertableType = typeof(DateTime);

        public override bool CanConvert(Type objectType)
        {
            return (null != objectType && convertableType == objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (null == value)
            {
                throw new Exception("Attempt to convert null DateTime value.");
            }
            else
            {
                DateTime dateTimeValue = (DateTime)value;
                writer.WriteValue(dateTimeValue.ToString(dateFormat, CultureInfo.InvariantCulture));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "Cannot convert null value to {0}.", objectType));
            }

            return DateTime.ParseExact(reader.Value.ToString(), dateFormat, CultureInfo.InvariantCulture);
        }
    }
}