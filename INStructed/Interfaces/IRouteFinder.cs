using System.Collections.Generic;

namespace INStructed.Interfaces
{
    public interface IRouteFinder
    {
        List<string> FindShortestPath(string start, string end);
    }
}
