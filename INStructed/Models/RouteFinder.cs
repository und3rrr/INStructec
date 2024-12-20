using INStructed.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace INStructed.Services
{
    public class RouteFinder : IRouteFinder
    {
        private readonly Dictionary<string, List<(string, int)>> _graph;

        public RouteFinder(Dictionary<string, List<(string, int)>> graph)
        {
            _graph = graph;
        }

        public List<string> FindShortestPath(string start, string end)
        {
            var distances = new Dictionary<string, int>();
            var previous = new Dictionary<string, string>();
            var unvisited = new HashSet<string>(_graph.Keys);

            foreach (var vertex in _graph.Keys)
            {
                distances[vertex] = int.MaxValue;
            }
            distances[start] = 0;

            while (unvisited.Count > 0)
            {
                var current = unvisited.OrderBy(v => distances[v]).First();
                unvisited.Remove(current);

                if (current == end)
                {
                    var path = new List<string>();
                    while (current != null)
                    {
                        path.Insert(0, current);
                        previous.TryGetValue(current, out current);
                    }
                    return path;
                }

                foreach (var (neighbor, weight) in _graph[current])
                {
                    if (!unvisited.Contains(neighbor)) continue;

                    int newDist = distances[current] + weight;
                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        previous[neighbor] = current;
                    }
                }
            }

            return null;
        }
    }
}
