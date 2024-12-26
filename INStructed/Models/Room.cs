using INStructed.Interfaces;
using System.Drawing;

namespace INStructed.Models
{
    public class Room : IRoom
    {
        public string Name { get; }
        public Point Coordinates { get; }
        public int FloorId { get; } // Добавили свойство FloorId

        public Room(string name, Point coordinates, int floorId)
        {
            Name = name;
            Coordinates = coordinates;
            FloorId = floorId; // Инициализируем FloorId
        }
    }
}
