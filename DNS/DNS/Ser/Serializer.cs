using System.Collections.Generic;
using System.IO;
using System.Linq;
using Makaretu.Dns;
using Newtonsoft.Json;

namespace DNS.Ser
{

    class Serializer
    {
        private JsonSerializerSettings serializeSettings;

        public Serializer()
        {
            serializeSettings = new JsonSerializerSettings();
            serializeSettings.Converters.Add(new IPAddressConverter());
            serializeSettings.Converters.Add(new IPEndPointConverter());
            serializeSettings.Converters.Add(new DnsDomainNameConverter());
            serializeSettings.TypeNameHandling = TypeNameHandling.All;
            serializeSettings.Formatting = Formatting.Indented;

        }

        public void Save(ResourceRecord[] info)
        {
            using (var output = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Settings.json")))
                output.Write(JsonConvert.SerializeObject(info, serializeSettings));
        }

        public  void Load(DnsServer server)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Settings.json");

            if (!File.Exists(path))
                return;

            using (var inp = new StreamReader(path))
            {
                var records = JsonConvert.DeserializeObject<ResourceRecord[]>(inp.ReadToEnd(), serializeSettings);
                if (records != null)
                    foreach (var resourceRecord in records)
                        server.Add(resourceRecord);
            }
        }
    }
}
