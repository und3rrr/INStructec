using System.Drawing;

namespace INStructed.Models

{
    public class Room
    {
        public string Name { get; }
        public Point Coordinates { get; }

        public Room(string name, Point coordinates)
        {
            Name = name;
            Coordinates = coordinates;
        }
    }
}
