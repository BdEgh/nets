using System;
using Makaretu.Dns;
using Newtonsoft.Json;

namespace DNS.Ser
{
    public class DnsDomainNameConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(DomainName));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = (DomainName)value;
            writer.WriteValue(obj.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var a = (string) reader.Value;
            return (DomainName)((string)reader.Value);
        }
    }
}