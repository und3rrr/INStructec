using INStructed.Interfaces;
using System.Drawing;

namespace INStructed.Interfaces
{
    public interface IRoom
    {
        string Name { get; }
        Point Coordinates { get; }
        int FloorId { get; } // Добавили свойство FloorId
    }
}