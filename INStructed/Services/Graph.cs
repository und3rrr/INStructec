// Graph.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace INStructed.Services
{
    /// <summary>
    /// Шаблонный класс транспортной сети, представляющий граф.
    /// </summary>
    /// <typeparam name="TNode">Тип узлов графа.</typeparam>
    /// <typeparam name="TEdge">Тип рёбер графа (например, вес).</typeparam>
    public class Graph<TNode, TEdge>
    {
        /// <summary>
        /// Список узлов графа.
        /// </summary>
        private readonly HashSet<TNode> nodes;

        /// <summary>
        /// Список рёбер графа и их весов.
        /// </summary>
        private readonly Dictionary<TNode, List<(TNode Destination, TEdge Edge)>> edges;

        /// <summary>
        /// Конструктор графа.
        /// </summary>
        public Graph()
        {
            nodes = new HashSet<TNode>();
            edges = new Dictionary<TNode, List<(TNode, TEdge)>>();
        }

        /// <summary>
        /// Добавляет узел в граф.
        /// </summary>
        /// <param name="node">Узел для добавления.</param>
        public void AddNode(TNode node)
        {
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
                edges[node] = new List<(TNode, TEdge)>();
            }
        }

        /// <summary>
        /// Добавляет вершину в граф.
        /// </summary>
        /// <param name="node">Вершина для добавления.</param>
        public void AddVertex(TNode node)
        {
            AddNode(node);
        }

        /// <summary>
        /// Добавляет направленное ребро между двумя узлами.
        /// </summary>
        /// <param name="source">Начальный узел.</param>
        /// <param name="destination">Конечный узел.</param>
        /// <param name="edge">Ребро (например, вес).</param>
        public void AddEdge(TNode source, TNode destination, TEdge edge)
        {
            if (!nodes.Contains(source) || !nodes.Contains(destination))
                throw new InvalidOperationException("Оба узла должны существовать в графе.");

            edges[source].Add((destination, edge));
        }

        /// <summary>
        /// Находит кратчайший путь между двумя узлами с использованием алгоритма Дейкстры.
        /// </summary>
        /// <param name="start">Начальный узел.</param>
        /// <param name="end">Конечный узел.</param>
        /// <param name="getWeight">Функция для получения веса из ребра.</param>
        /// <returns>Список узлов, представляющий кратчайший путь.</returns>
        public List<TNode> FindShortestPath(TNode start, TNode end, Func<TEdge, double> getWeight)
        {
            var distances = nodes.ToDictionary(node => node, node => double.PositiveInfinity);
            var previous = new Dictionary<TNode, TNode>();
            var priorityQueue = new SortedSet<(double, TNode)>(Comparer<(double, TNode)>.Create((a, b) =>
            {
                int compare = a.Item1.CompareTo(b.Item1);
                if (compare != 0) return compare;
                return Comparer<TNode>.Default.Compare(a.Item2, b.Item2);
            }));

            distances[start] = 0;
            priorityQueue.Add((0, start));

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Min;
                priorityQueue.Remove(current);

                double currentDistance = current.Item1;
                TNode currentNode = current.Item2;

                if (currentNode.Equals(end))
                    break;

                foreach (var (neighbor, edge) in edges[currentNode])
                {
                    double weight = getWeight(edge);
                    double distance = currentDistance + weight;

                    if (distance < distances[neighbor])
                    {
                        var existing = (distances[neighbor], neighbor);
                        if (priorityQueue.Contains(existing))
                        {
                            priorityQueue.Remove(existing);
                        }

                        distances[neighbor] = distance;
                        previous[neighbor] = currentNode;
                        priorityQueue.Add((distance, neighbor));
                    }
                }
            }

            var path = new List<TNode>();
            var at = end;
            if (previous.ContainsKey(at) || at.Equals(start))
            {
                while (!at.Equals(default(TNode)) && previous.ContainsKey(at))
                {
                    path.Add(at);
                    at = previous[at];
                }

                if (at.Equals(start))
                {
                    path.Add(start);
                    path.Reverse();
                }
            }

            return path.FirstOrDefault()?.Equals(start) == true ? path : new List<TNode>();
        }
    }
}
