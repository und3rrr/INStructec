using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace INStructed.Services
{
    public class PointConverter : JsonConverter<Point>
    {
        public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);
            if (array.Count != 2)
                throw new JsonSerializationException("Invalid point format. Expected [x, y].");

            int x = array[0].Value<int>();
            int y = array[1].Value<int>();
            return new Point(x, y);
        }

        public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
        {
            JArray array = new JArray { value.X, value.Y };
            array.WriteTo(writer);
        }
    }
}
