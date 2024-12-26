using System;
using System.Collections.Generic;

namespace INStructed.Services
{
    public class Graph<T>
    {
        private Dictionary<T, List<(T, int)>> _adjacencyList = new Dictionary<T, List<(T, int)>>();

        // Добавление вершины и рёбер
        public void AddVertex(T vertex)
        {
            if (!_adjacencyList.ContainsKey(vertex))
            {
                _adjacencyList[vertex] = new List<(T, int)>();
            }
        }

        public void AddEdge(T vertex1, T vertex2, int weight = 1)
        {
            if (!_adjacencyList.ContainsKey(vertex1))
                AddVertex(vertex1);
            if (!_adjacencyList.ContainsKey(vertex2))
                AddVertex(vertex2);

            _adjacencyList[vertex1].Add((vertex2, weight));
            _adjacencyList[vertex2].Add((vertex1, weight));  // Граф неориентированный
        }

        // Алгоритм Дейкстры для нахождения кратчайшего пути
        public List<T> Dijkstra(T start, T end)
        {
            var distances = new Dictionary<T, int>();
            var previous = new Dictionary<T, T>();
            var queue = new SortedSet<(int, T)>();

            foreach (var vertex in _adjacencyList.Keys)
            {
                distances[vertex] = int.MaxValue;
                previous[vertex] = default;
                queue.Add((distances[vertex], vertex));
            }

            distances[start] = 0;
            queue.Add((0, start));

            while (queue.Count > 0)
            {
                var current = queue.Min.Item2;
                queue.Remove(queue.Min);

                if (EqualityComparer<T>.Default.Equals(current, end))
                {
                    break;
                }

                foreach (var neighbor in _adjacencyList[current])
                {
                    var alt = distances[current] + neighbor.Item2;
                    if (alt < distances[neighbor.Item1])
                    {
                        if (queue.Contains((distances[neighbor.Item1], neighbor.Item1)))
                            queue.Remove((distances[neighbor.Item1], neighbor.Item1));

                        distances[neighbor.Item1] = alt;
                        previous[neighbor.Item1] = current;
                        queue.Add((distances[neighbor.Item1], neighbor.Item1));
                    }
                }
            }

            // Восстановление пути
            var path = new List<T>();
            if (!previous.ContainsKey(end) && !EqualityComparer<T>.Default.Equals(end, default(T)))
                return path; // Маршрут не найден

            for (T at = end; !EqualityComparer<T>.Default.Equals(at, default(T)); at = previous[at])
            {
                path.Insert(0, at);
                if (EqualityComparer<T>.Default.Equals(previous[at], at))
                    break; // Предотвращаем циклы
            }

            return path;
        }

    }
}