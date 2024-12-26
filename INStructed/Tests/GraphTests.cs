using System.Collections.Generic;
using INStructed.Services;
using Xunit;

namespace INStructed.Tests
{
    public class GraphTests
    {
        [Fact]
        public void Test_AddVertex_AddsVertexToGraph()
        {
            // Arrange
            var graph = new Graph<string>();

            // Act
            graph.AddVertex("A");

            // Assert
            var result = graph.Dijkstra("A", "A");
            Assert.Single(result);
            Assert.Equal("A", result[0]);
        }

        [Fact]
        public void Test_AddEdge_CreatesConnectionBetweenVertices()
        {
            // Arrange
            var graph = new Graph<string>();

            // Act
            graph.AddEdge("A", "B", 5);

            // Assert
            var path = graph.Dijkstra("A", "B");
            Assert.Equal(new List<string> { "A", "B" }, path);
        }

        [Fact]
        public void Test_Dijkstra_FindsShortestPath()
        {
            // Arrange
            var graph = new Graph<string>();
            graph.AddEdge("A", "B", 1);
            graph.AddEdge("B", "C", 2);
            graph.AddEdge("A", "C", 5);

            // Act
            var path = graph.Dijkstra("A", "C");

            // Assert
            Assert.Equal(new List<string> { "A", "B", "C" }, path);
        }

        [Fact]
        public void Test_Dijkstra_ReturnsEmptyPathForDisconnectedGraph()
        {
            // Arrange
            var graph = new Graph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");

            // Act
            var path = graph.Dijkstra("A", "B");

            // Assert
            Assert.Empty(path);
        }
    }
}
