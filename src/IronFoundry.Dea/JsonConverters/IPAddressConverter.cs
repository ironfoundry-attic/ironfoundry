namespace IronFoundry.Dea.JsonConverters
{
    using System;
    using System.Globalization;
    using System.Net;
    using Newtonsoft.Json;

    public class IPAddressConverter : JsonConverter
    {
        private static readonly Type convertableType = typeof(IPAddress);

        public override bool CanConvert(Type objectType)
        {
            return convertableType == objectType;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object rv;

            if (reader.TokenType == JsonToken.Null)
            {
                rv = null;
            }
            else
            {
                reader.Read();

                if (JsonToken.String != reader.TokenType)
                {
                    throw new Exception(
                        String.Format(CultureInfo.InvariantCulture,
                        "Unexpected token parsing IP Address. Expected String, got {0}.", reader.TokenType));
                }

                IPAddress addr;
                if (IPAddress.TryParse((string)reader.Value, out addr))
                {
                    rv = addr;
                }
                else
                {
                    throw new Exception(
                        String.Format(CultureInfo.InvariantCulture,
                        "Could not parse '{0}' as IP Address.", reader.Value));
                }
            }

            return rv;
        }
    }
}