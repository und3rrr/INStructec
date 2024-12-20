using System.Collections.Generic;
using System.Drawing;

namespace INStructed.Interfaces
{
    public interface IMapRenderer
    {
        void Render(Graphics g, Dictionary<string, Point> roomCoordinates, List<string> route);
    }
}
