using System.Collections.Generic;

namespace INStructed.Models

{
    public interface IFloor
    {
        int Id { get; }
        string Name { get; }
        Dictionary<string, Room> Rooms { get; }
        List<(string, string)> Connections { get; }
    }
}
