using INStructed.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace INStructed.Models
{
    public class Room : IComparable<Room>
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("floorId")]
        public int FloorId { get; set; }

        [JsonProperty("coordinates")]
        [JsonConverter(typeof(INStructed.Services.PointConverter))]
        public Point Coordinates { get; set; }

        public int CompareTo(Room other)
        {
            return string.Compare(this.Name, other.Name, StringComparison.Ordinal);
        }
    }
}
