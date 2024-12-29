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
            // Инициализация расстояний до всех узлов как бесконечность
            var distances = nodes.ToDictionary(node => node, node => double.PositiveInfinity);
            distances[start] = 0;

            // Словарь для отслеживания предыдущих узлов на пути
            var previous = new Dictionary<TNode, TNode>();

            // Инициализация приоритетной очереди
            var priorityQueue = new PriorityQueue<TNode, double>();
            priorityQueue.Enqueue(start, 0);

            // Множество посещённых узлов
            var visited = new HashSet<TNode>();

            while (!priorityQueue.IsEmpty)
            {
                // Извлекаем узел с минимальным расстоянием
                var currentNode = priorityQueue.Dequeue();

                // Если узел уже посещён, пропускаем
                if (visited.Contains(currentNode))
                    continue;

                // Помечаем узел как посещённый
                visited.Add(currentNode);

                // Если достигли конечного узла, выходим из цикла
                if (EqualityComparer<TNode>.Default.Equals(currentNode, end))
                    break;

                // Проходим по всем соседям текущего узла
                foreach (var (neighbor, edge) in edges[currentNode])
                {
                    double weight = getWeight(edge);
                    double distance = distances[currentNode] + weight;

                    // Если найдено более короткое расстояние до соседа
                    if (distance < distances[neighbor])
                    {
                        distances[neighbor] = distance;
                        previous[neighbor] = currentNode;
                        priorityQueue.Enqueue(neighbor, distance);
                    }
                }
            }

            // Восстанавливаем путь от конца к началу
            var path = new List<TNode>();
            var at = end;

            if (previous.ContainsKey(at) || EqualityComparer<TNode>.Default.Equals(at, start))
            {
                while (!EqualityComparer<TNode>.Default.Equals(at, default(TNode)) && previous.ContainsKey(at))
                {
                    path.Add(at);
                    at = previous[at];
                }

                // Добавляем начальный узел и переворачиваем путь
                if (EqualityComparer<TNode>.Default.Equals(at, start))
                {
                    path.Add(start);
                    path.Reverse();
                }
            }

            // Проверяем, начинается ли путь с начального узла
            return path.FirstOrDefault()?.Equals(start) == true ? path : new List<TNode>();
        }

        /// <summary>
        /// Получает вес рёбра между двумя узлами.
        /// </summary>
        /// <param name="source">Исходный узел.</param>
        /// <param name="destination">Конечный узел.</param>
        /// <returns>Вес рёбра.</returns>
        public TEdge GetEdgeWeight(TNode source, TNode destination)
        {
            if (edges.TryGetValue(source, out var neighbors))
            {
                foreach (var (dest, edge) in neighbors)
                {
                    if (EqualityComparer<TNode>.Default.Equals(dest, destination))
                    {
                        return edge;
                    }
                }
            }
            throw new InvalidOperationException($"Ребро от {source} до {destination} не существует.");
        }
    }
}
