using INStructed.Interfaces;
using System.Collections.Generic;

namespace INStructed.Models
{
    public class Floor : IFloor
    {
        public int Id { get; }
        public string Name { get; }
        public Dictionary<string, Room> Rooms { get; }
        public List<(string, string)> Connections { get; }

        public Floor(int id, string name, Dictionary<string, Room> rooms, List<(string, string)> connections)
        {
            Id = id;
            Name = name;
            Rooms = rooms;
            Connections = connections;
        }
    }
}