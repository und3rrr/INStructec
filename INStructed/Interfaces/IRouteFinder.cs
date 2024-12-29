// IRouteFinder.cs
using System.Collections.Generic;

namespace INStructed.Interfaces
{
    public interface IRouteFinder
    {
        /// <summary>
        /// Находит кратчайший путь между двумя точками.
        /// </summary>
        /// <param name="start">Начальная точка.</param>
        /// <param name="end">Конечная точка.</param>
        /// <returns>Список точек, представляющий кратчайший путь, или null если пути нет.</returns>
        List<string> FindShortestPath(string start, string end);
    }
}