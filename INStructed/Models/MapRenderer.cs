using INStructed.Interfaces;
using System.Collections.Generic;
using System.Drawing;

namespace INStructed.Models
{
    public class MapRenderer : IMapRenderer
    {
        public void Render(Graphics g, Dictionary<string, Point> roomCoordinates, List<string> route)
        {
            // Рисуем комнаты
            foreach (var room in roomCoordinates)
            {
                g.FillEllipse(Brushes.LightBlue, room.Value.X - 10, room.Value.Y - 10, 20, 20);
                g.DrawString(room.Key, SystemFonts.DefaultFont, Brushes.Black, room.Value.X - 20, room.Value.Y - 30);
            }

            // Рисуем линии между комнатами
            foreach (var room in roomCoordinates)
            {
                if (roomCoordinates.ContainsKey("Гардероб"))
                    g.DrawLine(Pens.Black, room.Value, roomCoordinates["Гардероб"]);
            }

            // Рисуем маршрут
            if (route != null && route.Count > 1)
            {
                for (int i = 0; i < route.Count - 1; i++)
                {
                    var start = roomCoordinates[route[i]];
                    var end = roomCoordinates[route[i + 1]];
                    g.DrawLine(Pens.Red, start, end);
                }
            }
        }
    }
}
