// GraphTests.cs
using System;
using System.Collections.Generic;
using INStructed.Services;
using Xunit;

namespace INStructed.Services.Tests
{
    public class GraphTests
    {
        private Graph<string, int> graph;

        public GraphTests()
        {
            graph = new Graph<string, int>();
        }

        [Fact]
        public void AddNode_ShouldAddNode()
        {
            graph.AddNode("A");
            var path = graph.FindShortestPath("A", "B", edge => edge);

            Assert.Empty(path); // Путь должен быть пустым, так как узел "B" не добавлен
        }

        [Fact]
        public void AddEdge_ShouldAddEdge()
        {
            graph.AddNode("A");
            graph.AddNode("B");
            graph.AddEdge("A", "B", 5);

            var path = graph.FindShortestPath("A", "B", edge => edge);
            Assert.Equal(2, path.Count); // Путь должен содержать 2 узла: "A" и "B"
            Assert.Equal("A", path[0]);
            Assert.Equal("B", path[1]);

            // Проверяем вес пути
            double totalWeight = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
                var edgeWeight = graph.GetEdgeWeight(current, next);
                totalWeight += edgeWeight;
            }
            Assert.Equal(5, totalWeight);
        }

        [Fact]
        public void AddEdge_ShouldThrowException_WhenNodesDoNotExist()
        {
            graph.AddNode("A");
            var exception = Assert.Throws<InvalidOperationException>(() => graph.AddEdge("A", "B", 5));
            Assert.Equal("Оба узла должны существовать в графе.", exception.Message);
        }

        [Fact]
        public void FindShortestPath_ShouldReturnCorrectPath()
        {
            graph.AddNode("A");
            graph.AddNode("B");
            graph.AddNode("C");
            graph.AddEdge("A", "B", 1);
            graph.AddEdge("B", "C", 2);
            graph.AddEdge("A", "C", 4);

            var path = graph.FindShortestPath("A", "C", edge => edge);
            Assert.Equal(3, path.Count); // Теперь путь должен содержать 3 узла: "A", "B", и "C"
            Assert.Equal("A", path[0]);
            Assert.Equal("B", path[1]);
            Assert.Equal("C", path[2]);

            // Проверяем общий вес пути
            double totalWeight = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
                var edgeWeight = graph.GetEdgeWeight(current, next);
                totalWeight += edgeWeight;
            }
            Assert.Equal(3, totalWeight); // 1 + 2 = 3
        }

        [Fact]
        public void FindShortestPath_ShouldReturnEmptyList_WhenNoPathExists()
        {
            graph.AddNode("A");
            graph.AddNode("B");
            graph.AddNode("C");
            graph.AddEdge("A", "B", 1);
            // Нет ребер к "C"

            var path = graph.FindShortestPath("A", "C", edge => edge);
            Assert.Empty(path); // Путь должен быть пустым, так как нет пути от "A" к "C"
        }

        [Fact]
        public void FindShortestPath_ShouldHandleCyclesCorrectly()
        {

            graph.AddNode("A");
            graph.AddNode("B");
            graph.AddNode("C");
            graph.AddNode("D");
            graph.AddNode("E");
            graph.AddNode("F");

            // Добавляем рёбра с весами
            graph.AddEdge("A", "B", 1);
            graph.AddEdge("B", "C", 2);
            graph.AddEdge("C", "F", 3);
            graph.AddEdge("A", "D", 4);
            graph.AddEdge("D", "E", 1);
            graph.AddEdge("E", "B", 1);
            graph.AddEdge("E", "F", 2);

            var path = graph.FindShortestPath("A", "F", edge => edge);

            var expectedPath = new List<string> { "A", "B", "C", "F" };
            Assert.Equal(expectedPath.Count, path.Count);
            for (int i = 0; i < expectedPath.Count; i++)
            {
                Assert.Equal(expectedPath[i], path[i]);
            }

            // Дополнительно проверяем общий вес пути
            double totalWeight = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
                var edgeWeight = graph.GetEdgeWeight(current, next);
                totalWeight += edgeWeight;
            }
            Assert.Equal(6, totalWeight); // 1 (A->B) + 2 (B->C) + 3 (C->F) = 6
        }
    }
}