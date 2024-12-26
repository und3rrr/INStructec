using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INStructed.Interfaces
{
    public class Building
    {
        [JsonProperty("floors")]
        public List<Floor> Floors { get; set; }
    }
    // Кастомный конвертер для десериализации Connections
    public class ConnectionConverter : JsonConverter<List<(string, string)>>
    {
        public override List<(string, string)> ReadJson(JsonReader reader, Type objectType, List<(string, string)> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new List<(string, string)>();
            var rawConnections = serializer.Deserialize<List<List<string>>>(reader);

            foreach (var connection in rawConnections)
            {
                if (connection.Count == 2)
                {
                    result.Add((connection[0], connection[1]));
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, List<(string, string)> value, JsonSerializer serializer)
        {
            var rawConnections = new List<List<string>>();

            foreach (var connection in value)
            {
                rawConnections.Add(new List<string> { connection.Item1, connection.Item2 });
            }

            serializer.Serialize(writer, rawConnections);
        }
    }
}
