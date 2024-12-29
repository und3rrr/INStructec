// RouteFinder.cs
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
            // Проверка существования начальной и конечной комнат
            if (!_graph.ContainsKey(start) || !_graph.ContainsKey(end))
                return null;

            var distances = new Dictionary<string, int>();
            var previous = new Dictionary<string, string>();
            var unvisited = new HashSet<string>(_graph.Keys);

            foreach (var vertex in _graph.Keys)
            {
                distances[vertex] = int.MaxValue;
            }
            distances[start] = 0;

            var priorityQueue = new PriorityQueue<string, int>();
            priorityQueue.Enqueue(start, 0);

            while (!priorityQueue.IsEmpty)
            {
                var currentNode = priorityQueue.Dequeue();

                if (currentNode == end)
                {
                    var path = new List<string>();
                    while (currentNode != null)
                    {
                        path.Insert(0, currentNode);
                        previous.TryGetValue(currentNode, out currentNode);
                    }
                    return path;
                }

                unvisited.Remove(currentNode);

                foreach (var (neighbor, weight) in _graph[currentNode])
                {
                    if (!unvisited.Contains(neighbor)) continue;

                    int newDist = distances[currentNode] + weight;
                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        previous[neighbor] = currentNode;
                        priorityQueue.Enqueue(neighbor, newDist);
                    }
                }
            }

            return null;
        }
    }
}