using INStructed.Models;
using INStructed.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class Form : System.Windows.Forms.Form
{
    private ComboBox cmbFloors;
    private ComboBox cmbStartRoom;
    private ComboBox cmbEndRoom;
    private TextBox txtSearch;
    private ListBox lstSearchResults;
    private Panel pnlMap;
    private Button btnSearch;
    private Button btnFindRoute;
    private Dictionary<int, Floor> floors;
    private int currentFloor;
    private List<string> currentRoute;
    private float zoom = 1.0f;
    private float rotationAngle = 0f;
    private Point lastMousePosition;
    private bool isDragging = false;
    private Point mapOffset = new Point(0, 0);
    private Point lastRightMousePosition;
    private bool isRightButtonDown = false;
    private ToolTip toolTip = new ToolTip();
    private Button btnChangeFloor;
    private List<string> routeToStair;
    private List<string> routeFromStair;
    private string externalEndRoom;
    private int externalEndFloor;

    public Form()
    {
        InitializeComponent();
        InitializeFloors();
    }

    private void InitializeComponent()
    {
        // Кнопка "Перейти на этаж"
        btnChangeFloor = new Button
        {
            Text = "Перейти на этаж",
            Location = new Point(920, 20),
            Width = 150,
            Visible = false
        };
        btnChangeFloor.Click += BtnChangeFloor_Click;

        // Добавление кнопки на форму
        Controls.Add(btnChangeFloor);

        // Выбор этажа
        cmbFloors = new ComboBox
        {
            Location = new Point(20, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };
        cmbFloors.SelectedIndexChanged += CmbFloors_SelectedIndexChanged;

        // Выбор начальной комнаты
        cmbStartRoom = new ComboBox
        {
            Location = new Point(180, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };

        // Выбор конечной комнаты
        cmbEndRoom = new ComboBox
        {
            Location = new Point(340, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };

        // Текстбокс для поиска
        txtSearch = new TextBox
        {
            Location = new Point(500, 20),
            Width = 150
        };
        txtSearch.TextChanged += TxtSearch_TextChanged;

        // Список результатов поиска
        lstSearchResults = new ListBox
        {
            Location = new Point(500, 45),
            Width = 150,
            Height = 100,
            Visible = false
        };
        lstSearchResults.Click += LstSearchResults_Click;

        // Кнопка поиска
        btnSearch = new Button
        {
            Text = "Поиск",
            Location = new Point(660, 20),
            Width = 80
        };
        btnSearch.Click += BtnSearch_Click;

        // Кнопка "Построить маршрут"
        btnFindRoute = new Button
        {
            Text = "Построить маршрут",
            Location = new Point(750, 20),
            Width = 150
        };
        btnFindRoute.Click += BtnFindRoute_Click;

        // Панель карты
        pnlMap = new Panel
        {
            Location = new Point(20, 160),
            Size = new Size(900, 700),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };
        pnlMap.Paint += PnlMap_Paint;

        // Добавление элементов на форму
        Controls.Add(cmbFloors);
        Controls.Add(cmbStartRoom);
        Controls.Add(cmbEndRoom);
        Controls.Add(txtSearch);
        Controls.Add(lstSearchResults);
        Controls.Add(btnSearch);
        Controls.Add(btnFindRoute);
        Controls.Add(pnlMap);

        // Настройка формы
        Text = "Навигация внутри здания";
        ClientSize = new Size(950, 900);

        // Обработка событий мыши
        pnlMap.MouseWheel += PnlMap_MouseWheel;
        pnlMap.MouseDown += PnlMap_MouseDown;
        pnlMap.MouseMove += PnlMap_MouseMove;
        pnlMap.MouseUp += PnlMap_MouseUp;
    }

    private void InitializeFloors()
    {
        floors = new Dictionary<int, Floor>
    {
        // Первый этаж
        {
            0, new Floor(0, "Первый этаж", new Dictionary<string, Room>
            {
                // Центральная часть
                { "Главный вход", new Room("Главный вход", new Point(300, 100), 0) },
                { "Гардероб", new Room("Гардероб", new Point(300, 150), 0) },

                // Левое крыло - Кабинеты
                { "101", new Room("101", new Point(250, 200), 0) },
                { "102", new Room("102", new Point(250, 250), 0) },
                { "103", new Room("103", new Point(250, 300), 0) },
                { "104", new Room("104", new Point(250, 350), 0) },
                { "105", new Room("105", new Point(250, 400), 0) },

                // Деканат и лестница
                { "Деканат", new Room("Деканат", new Point(350, 200), 0) },
                { "Лестница1_1", new Room("Лестница1_1", new Point(350, 250), 0) },
                { "106", new Room("106", new Point(350, 300), 0) },
                { "107", new Room("107", new Point(350, 350), 0) },

                // Правое крыло - Один кабинет, туалет и точка кипения
                { "109", new Room("109", new Point(400, 200), 0) },
                { "Туалет", new Room("Туалет", new Point(400, 250), 0) },
                { "Точка кипения", new Room("Точка кипения", new Point(450, 300), 0) }
            },
            new List<(string, string)>
            {
                // Центральные соединения
                ("Главный вход", "Гардероб"),

                // Левое крыло - Соединения кабинетов
                ("Гардероб", "101"),
                ("101", "102"),
                ("102", "103"),
                ("103", "104"),
                ("104", "105"),

                // Деканат и лестница - Соединения
                ("Гардероб", "Деканат"),
                ("Деканат", "Лестница1_1"),
                ("Лестница1_1", "106"),
                ("106", "107"),

                // Правое крыло - Соединения
                ("Гардероб", "109"),
                ("109", "Туалет"),
                ("Туалет", "Точка кипения"),

                // Дополнительные соединения для маршрутов
                ("Деканат", "105"),
                ("105", "107"),
                ("109", "Точка кипения")
            })
        },

        // Второй этаж
        {
            1, new Floor(1, "Второй этаж", new Dictionary<string, Room>
            {
                { "201", new Room("201", new Point(300, 500), 1) },
                { "202", new Room("202", new Point(350, 500), 1) },
                { "203", new Room("203", new Point(250, 550), 1) },
                { "204", new Room("204", new Point(350, 550), 1) },
                { "205", new Room("205", new Point(250, 600), 1) },
                { "206", new Room("206", new Point(350, 600), 1) },
                { "207", new Room("207", new Point(250, 650), 1) },
                { "208", new Room("208", new Point(350, 650), 1) },
                { "Лестница1_2", new Room("Лестница1_2", new Point(250, 700), 1) },
                { "215", new Room("215", new Point(450, 500), 1) },
                { "216", new Room("216", new Point(550, 500), 1) },
                { "217", new Room("217", new Point(450, 550), 1) },
                { "218", new Room("218", new Point(550, 550), 1) },
                { "Туалет", new Room("Туалет", new Point(550, 500), 1) },
                { "Лестница2_2", new Room("Лестница2_2", new Point(550, 650), 1) }
            },
            new List<(string, string)>
            {
                ("201", "203"), ("202", "204"),
                ("201", "215"), ("202", "216"),
                ("203", "205"), ("205", "207"),
                ("207", "Лестница1_2"),
                ("204", "206"), ("206", "208"),
                ("208", "Лестница1_2"),
                ("203", "204"), ("205", "206"), ("207", "208"),
                ("215", "217"), ("217", "218"),
                ("218", "Лестница2_2"),
                ("216", "218"), ("Туалет", "218"),
                ("Лестница1_2", "Лестница2_2")
            })
        }
    };

        // Заполнение ComboBox для этажей
        cmbFloors.Items.Clear();
        foreach (var floor in floors.Values)
        {
            cmbFloors.Items.Add(floor.Name);
        }
        cmbFloors.SelectedIndex = 0;
        currentFloor = 0;

        UpdateRoomSelectors();
    }

    private void UpdateRoomSelectors()
    {
        cmbStartRoom.Items.Clear();
        cmbEndRoom.Items.Clear();

        foreach (var room in floors[currentFloor].Rooms.Values)
        {
            cmbStartRoom.Items.Add(room.Name);
            cmbEndRoom.Items.Add(room.Name);
        }

        if (cmbStartRoom.Items.Count > 0) cmbStartRoom.SelectedIndex = 0;
        if (cmbEndRoom.Items.Count > 0) cmbEndRoom.SelectedIndex = 0;
    }

    private void BtnFindRoute_Click(object sender, EventArgs e)
    {
        var startRoom = cmbStartRoom.SelectedItem as string;
        string endRoom;
        int endFloor = currentFloor;

        if (!string.IsNullOrEmpty(externalEndRoom))
        {
            endRoom = externalEndRoom;
            endFloor = externalEndFloor;
        }
        else
        {
            endRoom = cmbEndRoom.SelectedItem as string;
        }

        if (startRoom == null || endRoom == null)
        {
            MessageBox.Show("Пожалуйста, выберите начальную и конечную комнаты.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var multiRoute = FindMultiFloorRoute(startRoom, endRoom);

        if (multiRoute.RouteToStair == null || multiRoute.RouteToStair.Count == 0)
        {
            MessageBox.Show("Маршрут не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        routeToStair = multiRoute.RouteToStair;
        routeFromStair = multiRoute.RouteFromStair;
        currentRoute = routeToStair;

        pnlMap.Invalidate();

        if (routeFromStair != null && routeFromStair.Count > 0)
        {
            // Получаем этаж назначения из первого элемента второго сегмента маршрута
            int targetFloorId = GetFloorId(routeFromStair.First());
            btnChangeFloor.Text = $"Перейти на этаж {floors[targetFloorId].Name}";
            btnChangeFloor.Visible = true;
        }
    }

    private void BtnChangeFloor_Click(object sender, EventArgs e)
    {
        if (routeFromStair == null || routeFromStair.Count == 0)
        {
            btnChangeFloor.Visible = false;
            return;
        }

        // Переключаем этаж на целевой
        cmbFloors.SelectedIndex = externalEndFloor;
        currentFloor = externalEndFloor;

        // Устанавливаем текущий маршрут на оставшуюся часть маршрута
        currentRoute = routeFromStair;
        routeFromStair = null;
        externalEndRoom = null;
        externalEndFloor = -1;

        pnlMap.Invalidate();

        // Скрываем кнопку после перехода
        btnChangeFloor.Visible = false;
    }
    private int GetFloorId(string roomName)
    {
        foreach (var floor in floors.Values)
        {
            if (floor.Rooms.ContainsKey(roomName))
                return floor.Id;
        }
        return -1;
    }

    // Исправлено: Удалены лямбда-выражения и корректное использование Dijkstra
    private (List<string> RouteToStair, List<string> RouteFromStair) FindMultiFloorRoute(string start, string end)
    {
        int startFloor = -1, endFloor = -1;
        foreach (var floor in floors.Values)
        {
            if (floor.Rooms.ContainsKey(start)) startFloor = floor.Id;
            if (floor.Rooms.ContainsKey(end)) endFloor = floor.Id;
        }

        if (startFloor == -1 || endFloor == -1)
            return (null, null);

        if (startFloor == endFloor)
        {
            var graph = BuildGraphForFloor(startFloor);
            var fullRoute = graph.Dijkstra(start, end);
            return (fullRoute, null);
        }
        else
        {
            string startStair = FindClosestStair(startFloor, start);
            string endStair = FindClosestStair(endFloor, end);

            if (startStair == null || endStair == null)
                return (null, null);

            string uniqueStartStair = GetUniqueStairName(floors[startFloor].Rooms[startStair], startFloor);
            string uniqueEndStair = GetUniqueStairName(floors[endFloor].Rooms[endStair], endFloor);

            var graphStart = BuildGraphForFloor(startFloor);
            var routeToStair = graphStart.Dijkstra(start, uniqueStartStair);
            if (routeToStair == null || routeToStair.Count == 0) return (null, null);

            var graphConnection = BuildConnectionGraph();
            var routeBetweenStairs = graphConnection.Dijkstra(uniqueStartStair, uniqueEndStair);
            if (routeBetweenStairs == null || routeBetweenStairs.Count == 0) return (null, null);

            var graphEnd = BuildGraphForFloor(endFloor);
            var routeFromStair = graphEnd.Dijkstra(uniqueEndStair, end);
            if (routeFromStair == null || routeFromStair.Count == 0) return (null, null);

            return (routeToStair, routeFromStair);
        }
    }

    private string FindClosestStair(int floorId, string room)
    {
        var floor = floors[floorId];
        var startRoom = floor.Rooms[room];
        var stairs = floor.Rooms.Values
            .Where(r => r.Name.StartsWith("Лестница", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!stairs.Any())
            return null;

        var closestStair = stairs
            .OrderBy(s => GetDistance(startRoom.Coordinates, s.Coordinates))
            .FirstOrDefault();

        return closestStair?.Name;
    }

    // Исправлено: Использование Graph<string> вместо Graph<string, int>
    private Graph<string> BuildConnectionGraph()
    {
        var graph = new Graph<string>();

        foreach (var floor in floors.Values)
        {
            foreach (var connection in floor.Connections)
            {
                graph.AddVertex(connection.Item1);
                graph.AddVertex(connection.Item2);
                int weight = (int)GetDistance(floor.Rooms[connection.Item1].Coordinates, floor.Rooms[connection.Item2].Coordinates);
                graph.AddEdge(connection.Item1, connection.Item2, weight);
            }
        }

        foreach (var floor in floors.Values)
        {
            var stairs = floor.Rooms.Values
                .Where(r => r.Name.StartsWith("Лестница", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var stair in stairs)
            {
                if (floors.ContainsKey(floor.Id + 1))
                {
                    var upperFloor = floors[floor.Id + 1];
                    var correspondingStair = upperFloor.Rooms.Values
                        .FirstOrDefault(r => r.Name.Split('_')[0].Equals(stair.Name.Split('_')[0], StringComparison.OrdinalIgnoreCase));

                    if (correspondingStair != null)
                    {
                        string stairNameCurrent = GetUniqueStairName(stair, floor.Id);
                        string stairNameUpper = GetUniqueStairName(correspondingStair, upperFloor.Id);

                        int weight = (int)GetDistance(stair.Coordinates, correspondingStair.Coordinates);
                        graph.AddEdge(stairNameCurrent, stairNameUpper, weight);
                    }
                }
            }
        }

        return graph;
    }

    // Исправлено: Использование Graph<string> вместо Graph<string, int>
    private Graph<string> BuildGraphForFloor(int floorId)
    {
        var floor = floors[floorId];
        var graph = new Graph<string>();

        foreach (var room in floor.Rooms.Values)
        {
            graph.AddVertex(room.Name);
        }

        foreach (var connection in floor.Connections)
        {
            string from = floor.Rooms.ContainsKey(connection.Item1) && connection.Item1.StartsWith("Лестница")
                          ? GetUniqueStairName(floor.Rooms[connection.Item1], floorId)
                          : connection.Item1;
            string to = floor.Rooms.ContainsKey(connection.Item2) && connection.Item2.StartsWith("Лестница")
                        ? GetUniqueStairName(floor.Rooms[connection.Item2], floorId)
                        : connection.Item2;

            int weight = (int)GetDistance(floor.Rooms[connection.Item1].Coordinates, floor.Rooms[connection.Item2].Coordinates);
            graph.AddEdge(from, to, weight);
        }

        return graph;
    }

    private float GetDistance(Point p1, Point p2)
    {
        return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }

    // Исправлено: Проверка переменных как int, а не как методы
    private void CmbFloors_SelectedIndexChanged(object sender, EventArgs e)
    {
        currentFloor = cmbFloors.SelectedIndex;
        UpdateRoomSelectors();
        pnlMap.Invalidate();
    }

    private string GetUniqueStairName(Room stair, int floorId)
    {
        // Изменяем формат имен лестниц: "Лестница1_1", "Лестница2_1", "Лестница1_2", "Лестница2_2"
        var parts = stair.Name.Split('_');
        if (parts.Length >= 1)
        {
            // Если имя уже содержит номер, заменяем его
            if (parts.Length == 2 && int.TryParse(parts[1], out _))
            {
                return $"{parts[0]}_{floorId + 1}";
            }
            else
            {
                // Добавляем номер этажа
                return $"{stair.Name}_{floorId + 1}";
            }
        }
        return $"{stair.Name}_{floorId + 1}";
    }

    private void PnlMap_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var floor = floors[currentFloor];

        g.Clear(Color.White);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        g.TranslateTransform(mapOffset.X, mapOffset.Y);
        g.ScaleTransform(zoom, zoom);
        g.RotateTransform(rotationAngle);

        foreach (var room in floor.Rooms.Values)
        {
            var roomCoords = room.Coordinates;
            g.FillEllipse(Brushes.LightBlue, roomCoords.X - 10, roomCoords.Y - 10, 20, 20);
            g.DrawEllipse(Pens.Black, roomCoords.X - 10, roomCoords.Y - 10, 20, 20);
            g.DrawString(room.Name, this.Font, Brushes.Black, roomCoords.X - 20, roomCoords.Y - 30);
        }

        if (currentRoute != null && currentRoute.Count > 1)
        {
            using (var pen = new Pen(Color.Red, 3))
            {
                for (int i = 0; i < currentRoute.Count - 1; i++)
                {
                    var currentRoom = GetRoomByName(currentRoute[i], out int floorId1);
                    var nextRoom = GetRoomByName(currentRoute[i + 1], out int floorId2);

                    if (currentRoom != null && nextRoom != null && floorId1 == currentFloor)
                    {
                        g.DrawLine(pen, currentRoom.Coordinates, nextRoom.Coordinates);
                        DrawArrow(g, pen, currentRoom.Coordinates, nextRoom.Coordinates);
                    }
                }
            }
        }
    }

    private Room GetRoomByName(string name, out int floorId)
    {
        foreach (var floor in floors.Values)
        {
            if (floor.Rooms.ContainsKey(name))
            {
                floorId = floor.Id;
                return floor.Rooms[name];
            }
        }
        floorId = -1;
        return null;
    }

    private void DrawArrow(Graphics g, Pen pen, Point start, Point end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        var angle = Math.Atan2(deltaY, deltaX);

        int arrowSize = 10;

        PointF arrowPoint1 = new PointF(
            end.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
            end.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6));

        PointF arrowPoint2 = new PointF(
            end.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
            end.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6));

        PointF[] arrowPoints = { end, arrowPoint1, arrowPoint2 };
        g.FillPolygon(Brushes.Red, arrowPoints);
    }

    private void PnlMap_MouseWheel(object sender, MouseEventArgs e)
    {
        if (e.Delta > 0)
            zoom *= 1.1f;
        else
            zoom /= 1.1f;

        pnlMap.Invalidate();
    }

    private void PnlMap_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            lastMousePosition = e.Location;
            isDragging = true;
        }
        else if (e.Button == MouseButtons.Right)
        {
            lastRightMousePosition = e.Location;
            isRightButtonDown = true;
        }
    }

    private void PnlMap_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            int deltaX = e.X - lastMousePosition.X;
            int deltaY = e.Y - lastMousePosition.Y;
            mapOffset.X += deltaX;
            mapOffset.Y += deltaY;
            lastMousePosition = e.Location;
            pnlMap.Invalidate();
        }
        else if (isRightButtonDown)
        {
            var deltaX = e.X - lastRightMousePosition.X;
            rotationAngle += deltaX * 0.1f;

            lastRightMousePosition = e.Location;
            pnlMap.Invalidate();
        }

        bool found = false;
        foreach (var room in floors[currentFloor].Rooms.Values)
        {
            // Преобразуем координаты с учетом трансформаций
            var transformedPoint = TransformPoint(room.Coordinates);
            var rect = new Rectangle((int)(transformedPoint.X - 10 * zoom), (int)(transformedPoint.Y - 10 * zoom), (int)(20 * zoom), (int)(20 * zoom));
            if (rect.Contains(e.Location))
            {
                toolTip.SetToolTip(pnlMap, room.Name);
                found = true;
                break;
            }
        }
        if (!found)
        {
            toolTip.SetToolTip(pnlMap, string.Empty);
        }
    }

    private Point TransformPoint(Point p)
    {
        // Обратное преобразование для определения положения курсора
        System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
        matrix.Translate(mapOffset.X, mapOffset.Y);
        matrix.Scale(zoom, zoom);
        matrix.Rotate(rotationAngle);
        Point[] pts = { p };
        matrix.TransformPoints(pts);
        return pts[0];
    }

    private void PnlMap_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            isDragging = false;
        else if (e.Button == MouseButtons.Right)
            isRightButtonDown = false;
    }

    private void TxtSearch_TextChanged(object sender, EventArgs e)
    {
        string query = txtSearch.Text.Trim();
        if (string.IsNullOrEmpty(query))
        {
            lstSearchResults.Visible = false;
            return;
        }

        var results = new List<string>();
        foreach (var floor in floors.Values)
        {
            results.AddRange(floor.Rooms.Keys
             .Where(r => r.StartsWith(query, StringComparison.OrdinalIgnoreCase))
             .Select(r => $"{floor.Name} - {r}"));

        }

        if (results.Count > 0)
        {
            lstSearchResults.DataSource = results.Distinct().ToList();
            lstSearchResults.Visible = true;
        }
        else
        {
            lstSearchResults.Visible = false;
        }
    }

    private void LstSearchResults_Click(object sender, EventArgs e)
    {
        if (lstSearchResults.SelectedItem != null)
        {
            string selectedItem = lstSearchResults.SelectedItem.ToString();
            var parts = selectedItem.Split(new string[] { " - " }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                string floorName = parts[0];
                string roomName = parts[1];

                // Найти этаж по имени
                var selectedFloor = floors.Values.FirstOrDefault(f => f.Name.Equals(floorName, StringComparison.OrdinalIgnoreCase));
                if (selectedFloor != null)
                {
                    if (selectedFloor.Id == currentFloor)
                    {
                        cmbEndRoom.SelectedItem = roomName;
                        txtSearch.Text = roomName;
                    }
                    else
                    {
                        externalEndRoom = roomName;
                        externalEndFloor = selectedFloor.Id;
                        txtSearch.Text = $"{selectedFloor.Id + 1}-{roomName}";

                        btnChangeFloor.Text = $"Перейти на этаж {selectedFloor.Name}";
                        btnChangeFloor.Visible = true;
                    }

                    lstSearchResults.Visible = false;
                }
            }
        }
    }

    private void BtnSearch_Click(object sender, EventArgs e)
    {
        string room = txtSearch.Text.Trim();
        if (string.IsNullOrEmpty(room))
        {
            MessageBox.Show("Введите номер кабинета для поиска.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Найти этаж и комнату
        foreach (var floor in floors.Values)
        {
            if (floor.Rooms.ContainsKey(room))
            {
                cmbFloors.SelectedIndex = floor.Id;
                cmbEndRoom.SelectedItem = room;
                pnlMap.Invalidate();
                return;
            }
        }

        MessageBox.Show("Кабинет не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form());
    }
}