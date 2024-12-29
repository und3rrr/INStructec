using System;
using System.Collections.Generic;
using INStructed.Services;
using Xunit;

namespace INStructed.Services.Tests
{
    public class RouteFinderTests
    {
        /// <summary>
        /// Тестирование валидации входных данных.
        /// Проверка, что метод возвращает null, если указаны несуществующие комнаты.
        /// </summary>
        [Fact]
        public void FindShortestPath_ShouldReturnNull_WhenRoomsDoNotExist()
        {
            // Arrange
            var graph = new Dictionary<string, List<(string, int)>>()
            {
                { "RoomA", new List<(string, int)> { ("RoomB", 5) } },
                { "RoomB", new List<(string, int)> { ("RoomC", 10) } },
                { "RoomC", new List<(string, int)> () }
            };
            var routeFinder = new RouteFinder(graph);

            // Act
            var path1 = routeFinder.FindShortestPath("RoomX", "RoomC"); // Начальная комната не существует
            var path2 = routeFinder.FindShortestPath("RoomA", "RoomY"); // Конечная комната не существует
            var path3 = routeFinder.FindShortestPath("RoomX", "RoomY"); // Обе комнаты не существуют

            // Assert
            Assert.Null(path1);
            Assert.Null(path2);
            Assert.Null(path3);
        }

        /// <summary>
        /// Тестирование маршрутов, пересекающих этажи через лестницы.
        /// Убедиться, что маршрут учитывает лестницы между этажами.
        /// </summary>
        [Fact]
        public void FindShortestPath_ShouldHandleStairsBetweenFloors()
        {
            // Arrange
            var graph = new Dictionary<string, List<(string, int)>>()
            {
                // Первый этаж
                { "Room1F_A", new List<(string, int)> { ("Stairs1F", 2) } },
                { "Room1F_B", new List<(string, int)> { ("Stairs1F", 3) } },
                { "Stairs1F", new List<(string, int)> { ("Stairs2F", 2) } },
                // Второй этаж
                { "Stairs2F", new List<(string, int)> { ("Room2F_A", 2), ("Room2F_B", 3) } },
                { "Room2F_A", new List<(string, int)> () },
                { "Room2F_B", new List<(string, int)> () }
            };
            var routeFinder = new RouteFinder(graph);

            // Act
            var path = routeFinder.FindShortestPath("Room1F_A", "Room2F_B");

            // Assert
            var expectedPath = new List<string> { "Room1F_A", "Stairs1F", "Stairs2F", "Room2F_B" };
            Assert.NotNull(path);
            Assert.Equal(expectedPath.Count, path.Count);
            for (int i = 0; i < expectedPath.Count; i++)
            {
                Assert.Equal(expectedPath[i], path[i]);
            }
        }

        /// <summary>
        /// Тестирование длинных маршрутов, проходящих через большое количество узлов.
        /// Проверка, что метод справляется с длинными маршрутами.
        /// </summary>
        [Fact]
        public void FindShortestPath_ShouldHandleLongRoutes()
        {
            // Arrange
            var graph = new Dictionary<string, List<(string, int)>>();

            // Создаем линейный граф из 100 узлов: Node1 -> Node2 -> ... -> Node100
            for (int i = 1; i <= 100; i++)
            {
                string current = $"Node{i}";
                if (i < 100)
                {
                    string next = $"Node{i + 1}";
                    graph[current] = new List<(string, int)> { (next, 1) };
                }
                else
                {
                    graph[current] = new List<(string, int)>();
                }
            }

            var routeFinder = new RouteFinder(graph);

            // Act
            var path = routeFinder.FindShortestPath("Node1", "Node100");

            // Assert
            Assert.NotNull(path);
            Assert.Equal(100, path.Count);
            for (int i = 0; i < 100; i++)
            {
                Assert.Equal($"Node{i + 1}", path[i]);
            }
        }

        /// <summary>
        /// Тестирование случая, когда начальная и конечная точки совпадают.
        /// Проверка, что маршрут имеет длину 0 и содержит только одну точку.
        /// </summary>
        [Fact]
        public void FindShortestPath_ShouldReturnSingleNode_WhenStartAndEndAreSame()
        {
            // Arrange
            var graph = new Dictionary<string, List<(string, int)>>()
            {
                { "RoomA", new List<(string, int)> { ("RoomB", 5) } },
                { "RoomB", new List<(string, int)> { ("RoomC", 10) } },
                { "RoomC", new List<(string, int)> () }
            };
            var routeFinder = new RouteFinder(graph);

            // Act
            var path = routeFinder.FindShortestPath("RoomA", "RoomA");

            // Assert
            Assert.NotNull(path);
            Assert.Single(path);
            Assert.Equal("RoomA", path[0]);
        }
    }
}