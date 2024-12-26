using INStructed.Interfaces;
using INStructed.Models;
using INStructed.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public partial class MainForm : System.Windows.Forms.Form
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
    private bool showConnections = true;
    private Button btnToggleConnections;
    private Button btnEditMode;
    private Button btnSave;
    private bool isEditMode = false;
    private Room selectedRoom = null;
    private BuildingEditorService editorService;
    private Button btnAddConnection;
    private Button btnDeleteConnection;
    private Button btnAddRoom;
    private Point addRoomPosition;
    private Button btnCancelSelection;
    private Point lastMouseMapPosition;


    public MainForm()
    {
        InitializeComponent();
        editorService = new BuildingEditorService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config"));
        InitializeFloors();
        this.DoubleBuffered = true;
    }
    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
            return cp;
        }
    }

    private void InitializeComponent()
    {
        btnAddRoom = new Button
        {
            Text = "Добавить комнату",
            Location = new Point(920, 300),
            Width = 150,
            Visible = false
        };
        btnAddRoom.Click += BtnAddRoom_Click; 
        Controls.Add(btnAddRoom);

        // Создание панели добавления комнаты
        Panel pnlAddRoom = new Panel
        {
            Location = new Point(700, 20),
            Size = new Size(200, 100),
            Visible = false
        };

        TextBox txtNewRoomName = new TextBox
        {
            Text = "Название комнаты",
            Location = new Point(10, 10),
            Width = 180
        };

        Button btnConfirmAddRoom = new Button
        {
            Text = "Добавить",
            Location = new Point(10, 40),
            Width = 80
        };
        btnConfirmAddRoom.Click += (s, e) => AddNewRoom(txtNewRoomName.Text, pnlAddRoom);

        Button btnCancelAddRoom = new Button
        {
            Text = "Отмена",
            Location = new Point(110, 40),
            Width = 80
        };
        btnCancelAddRoom.Click += (s, e) => pnlAddRoom.Visible = false;

        pnlAddRoom.Controls.Add(txtNewRoomName);
        pnlAddRoom.Controls.Add(btnConfirmAddRoom);
        pnlAddRoom.Controls.Add(btnCancelAddRoom);
        Controls.Add(pnlAddRoom);

        // Кнопка "Добавить соединение"
        btnAddConnection = new Button
        {
            Text = "Добавить соединение",
            Location = new Point(920, 220),
            Width = 150,
            Visible = false
        };
        btnAddConnection.Click += BtnAddConnection_Click;

        Controls.Add(btnAddConnection);

        // Остальная инициализация компонентов
        btnDeleteConnection = new Button
        {
            Text = "Удалить соединение",
            Location = new Point(920, 260),
            Width = 150,
            Visible = false
        };
        btnDeleteConnection.Click += BtnDeleteConnection_Click;
        Controls.Add(btnDeleteConnection);

        // Кнопка "Редактировать"
        btnEditMode = new Button
        {
            Text = "Редактировать",
            Location = new Point(920, 140),
            Width = 150
        };
        btnEditMode.Click += BtnEditMode_Click;
        Controls.Add(btnEditMode);

        // Кнопка "Сохранить"
        btnSave = new Button
        {
            Text = "Сохранить",
            Location = new Point(920, 180),
            Width = 150,
            Visible = false
        };
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);

        // Добавление кнопки "Показать/Скрыть соединения"
        btnToggleConnections = new Button
        {
            Text = "Показать/Скрыть соединения",
            Location = new Point(920, 80),
            Width = 150,
            Visible = true
        };
        btnToggleConnections.Click += BtnToggleConnections_Click;
        Controls.Add(btnToggleConnections);

        // Кнопка "Перейти на этаж"
        btnChangeFloor = new Button
        {
            Text = "Перейти на этаж",
            Location = new Point(920, 20),
            Width = 150,
            Visible = false
        };
        btnChangeFloor.Click += BtnChangeFloor_Click;
        Controls.Add(btnChangeFloor);

        // Выбор этажа
        cmbFloors = new ComboBox
        {
            Location = new Point(20, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };
        cmbFloors.SelectedIndexChanged += CmbFloors_SelectedIndexChanged;
        Controls.Add(cmbFloors);

        // Выбор начальной комнаты
        cmbStartRoom = new ComboBox
        {
            Location = new Point(180, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };
        Controls.Add(cmbStartRoom);

        // Выбор конечной комнаты
        cmbEndRoom = new ComboBox
        {
            Location = new Point(340, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };
        Controls.Add(cmbEndRoom);

        // Текстбокс для поиска
        txtSearch = new TextBox
        {
            Location = new Point(500, 20),
            Width = 150
        };
        txtSearch.TextChanged += TxtSearch_TextChanged;
        Controls.Add(txtSearch);

        // Список результатов поиска
        lstSearchResults = new ListBox
        {
            Location = new Point(500, 45),
            Width = 150,
            Height = 100,
            Visible = false
        };
        lstSearchResults.Click += LstSearchResults_Click;
        Controls.Add(lstSearchResults);

        // Кнопка поиска
        btnSearch = new Button
        {
            Text = "Поиск",
            Location = new Point(660, 20),
            Width = 80
        };
        btnSearch.Click += BtnSearch_Click;
        Controls.Add(btnSearch);

        // Кнопка "Построить маршрут"
        btnFindRoute = new Button
        {
            Text = "Построить маршрут",
            Location = new Point(750, 20),
            Width = 150
        };
        btnFindRoute.Click += BtnFindRoute_Click;
        Controls.Add(btnFindRoute);

        // Панель карты
        pnlMap = new Panel
        {
            Location = new Point(20, 160),
            Size = new Size(900, 700),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            TabStop = true
        };

        btnCancelSelection = new Button
        {
            Text = "Отмена выбора",
            Location = new Point(920, 380), // Установите нужную позицию
            Width = 150,
            Visible = false
        };
        btnCancelSelection.Click += BtnCancelSelection_Click;
        Controls.Add(btnCancelSelection);

        pnlMap.Paint += PnlMap_Paint;
        pnlMap.MouseWheel += PnlMap_MouseWheel;
        pnlMap.MouseDown += PnlMap_MouseDown;
        pnlMap.MouseMove += PnlMap_MouseMove;
        pnlMap.MouseUp += PnlMap_MouseUp;
        Controls.Add(pnlMap);

        // Установка Tag для btnAddRoom
        btnAddRoom.Tag = pnlAddRoom;

        pnlMap.TabStop = true;
        pnlMap.Focus();

        // Добавление StatusStrip
        StatusStrip statusStrip = new StatusStrip();
        ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();
        statusStrip.Items.Add(statusLabel);
        Controls.Add(statusStrip);

        // Сделать панель доступной для обновления статуса
        this.Tag = statusLabel;

        // Настройка формы
        Text = "Навигация внутри здания";
        ClientSize = new Size(1100, 900);
    }

    private void InitializeFloors()
    {
        floors = new Dictionary<int, Floor>();

        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "buildings.json");
        if (!File.Exists(jsonPath))
        {
            MessageBox.Show("Файл buildings.json не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string jsonData = File.ReadAllText(jsonPath);
        try
        {
            var building = JsonConvert.DeserializeObject<Building>(jsonData);

            foreach (var floor in building.Floors)
            {
                floors.Add(floor.Id, floor);
            }

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
        catch (JsonException ex)
        {
            MessageBox.Show($"Ошибка десериализации JSON: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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

        if (showConnections)
        {
            using (var pen = new Pen(Color.Gray, 1))
            {
                foreach (var connection in floor.Connections)
                {
                    var fromRoom = GetRoomByName(connection.Item1, out _);
                    var toRoom = GetRoomByName(connection.Item2, out _);
                    if (fromRoom != null && toRoom != null)
                    {
                        g.DrawLine(pen, fromRoom.Coordinates, toRoom.Coordinates);
                        if (selectedRooms.Contains(fromRoom) && selectedRooms.Contains(toRoom))
                        {
                            using (var highlightPen = new Pen(Color.Red, 2))
                            {
                                g.DrawLine(highlightPen, fromRoom.Coordinates, toRoom.Coordinates);
                            }
                        }
                    }
                }
            }
        }

        foreach (var room in floor.Rooms.Values)
        {
            var roomCoords = room.Coordinates;
            Color fillColor = Color.LightBlue;

            if (selectedRooms.Contains(room))
            {
                if (pnlMap.Tag as string == "selecting_connection")
                    fillColor = Color.Orange; // Выделение при выборе соединений
                else if (pnlMap.Tag as string == "deleting_connection")
                    fillColor = Color.Red; // Выделение при удалении соединений
                else
                    fillColor = Color.Yellow; // Обычное выделение
            }

            using (Brush brush = new SolidBrush(fillColor))
            {
                g.FillEllipse(brush, roomCoords.X - 10, roomCoords.Y - 10, 20, 20);
            }
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

                    if (currentRoom != null && nextRoom != null && floorId1 == currentFloor && floorId2 == currentFloor)
                    {
                        g.DrawLine(pen, currentRoom.Coordinates, nextRoom.Coordinates);
                        DrawArrow(g, pen, currentRoom.Coordinates, nextRoom.Coordinates);
                    }
                }
            }
        }
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

        // Сбрасываем routeToStair
        routeToStair = null;
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

    // Исправлено: Удалены лямбда-выражения и корректное использование FindShortestPath
    // Исправлено: Использование Graph<string, double> вместо Graph<string, int>
    // Исправленные вызовы BuildGraphForFloor с добавлением круглых скобок
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
            var fullRoute = graph.FindShortestPath(start, end, edge => edge);
            return (fullRoute, null);
        }
        else
        {
            string startStair = FindClosestStair(startFloor, start);
            string endStair = FindClosestStair(endFloor, end);

            if (startStair == null || endStair == null)
                return (null, null);

            string uniqueStartStair = startStair;
            string uniqueEndStair = endStair;

            var graphStart = BuildGraphForFloor(startFloor);
            var routeToStair = graphStart.FindShortestPath(start, uniqueStartStair, edge => edge);
            if (routeToStair == null || routeToStair.Count == 0) return (null, null);

            var graphConnection = BuildConnectionGraph();
            var routeBetweenStairs = graphConnection.FindShortestPath(uniqueStartStair, uniqueEndStair, edge => edge);
            if (routeBetweenStairs == null || routeBetweenStairs.Count == 0) return (null, null);

            var graphEnd = BuildGraphForFloor(endFloor);
            var routeFromStair = graphEnd.FindShortestPath(uniqueEndStair, end, edge => edge);
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

    // Исправлено: Использование Graph<string, double> вместо Graph<string, int>
    private Graph<string, double> BuildConnectionGraph()
    {
        var graph = new Graph<string, double>();

        foreach (var floor in floors.Values)
        {
            foreach (var connection in floor.Connections)
            {
                graph.AddVertex(connection.Item1);
                graph.AddVertex(connection.Item2);
                double weight = GetDistance(floor.Rooms[connection.Item1].Coordinates, floor.Rooms[connection.Item2].Coordinates);
                graph.AddEdge(connection.Item1, connection.Item2, weight);
                graph.AddEdge(connection.Item2, connection.Item1, weight); // Добавлено обратное ребро
            }
        }

        // Соединение лестниц между этажами по базовому имени
        for (int floorId = 0; floorId < floors.Count - 1; floorId++)
        {
            var currentFloor = floors[floorId];
            var upperFloor = floors[floorId + 1];

            var currentStairs = currentFloor.Rooms.Values
                .Where(r => r.Name.StartsWith("Лестница", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var upperStairs = upperFloor.Rooms.Values
                .Where(r => r.Name.StartsWith("Лестница", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var stair in currentStairs)
            {
                // Ищем соответствующую лестницу в верхнем этаже по базовому имени
                var baseNameCurrent = GetBaseStairName(stair.Name);
                var correspondingStair = upperStairs
                    .FirstOrDefault(s => GetBaseStairName(s.Name).Equals(baseNameCurrent, StringComparison.OrdinalIgnoreCase));

                if (correspondingStair != null)
                {
                    string stairNameCurrent = stair.Name;
                    string stairNameUpper = correspondingStair.Name;

                    double weight = GetDistance(stair.Coordinates, correspondingStair.Coordinates);
                    graph.AddEdge(stairNameCurrent, stairNameUpper, weight);
                    graph.AddEdge(stairNameUpper, stairNameCurrent, weight); // Добавлено обратное ребро
                }
            }
        }

        return graph;
    }


    // Исправлено: Использование Graph<string, double> вместо Graph<string, int>
    private Graph<string, double> BuildGraphForFloor(int floorId)
    {
        var floor = floors[floorId];
        var graph = new Graph<string, double>();

        foreach (var room in floor.Rooms.Values)
        {
            graph.AddVertex(room.Name);
        }

        foreach (var connection in floor.Connections)
        {
            string from = connection.Item1;
            string to = connection.Item2;

            double weight = GetDistance(floor.Rooms[connection.Item1].Coordinates, floor.Rooms[connection.Item2].Coordinates);
            graph.AddEdge(from, to, weight);
            graph.AddEdge(to, from, weight); // Добавлено обратное ребро
        }

        return graph;
    }

    // Исправлено: Использование Graph<string, int> вместо Graph<string>

    private float GetDistance(Point p1, Point p2)
    {
        return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }

    // Исправлено: Сравнение переменных как int
    private void CmbFloors_SelectedIndexChanged(object sender, EventArgs e)
    {
        currentFloor = cmbFloors.SelectedIndex;
        UpdateRoomSelectors();
        pnlMap.Invalidate();
    }
    private string GetUniqueStairName(Room stair, int floorId)
    {
        // Возвращаем базовое имя лестницы без суффикса этажа для соответствия между этажами
        var parts = stair.Name.Split('_');
        if (parts.Length >= 1)
        {
            return parts[0];
        }
        return stair.Name;
    }

    private string GetBaseStairName(string stairName)
    {
        // Возвращаем базовое имя лестницы без суффикса этажа
        var parts = stairName.Split('_');
        if (parts.Length >= 2)
        {
            return $"{parts[0]}_{parts[1]}";
        }
        return stairName;
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
        if (isEditMode)
        {
            if (pnlMap.Tag as string == "selecting_connection")
            {
                // Логика соединения комнат
                SelectRoomForConnection(e.Location);
            }
            else if (pnlMap.Tag as string == "deleting_connection")
            {
                // Логика удаления соединения
                DeleteConnectionAtPoint(e.Location);
            }
            else
            {
                foreach (var room in floors[currentFloor].Rooms.Values)
                {
                    var rect = GetRoomRectangle(room);
                    if (rect.Contains(e.Location))
                    {
                        if (ModifierKeys.HasFlag(Keys.Control))
                        {
                            // Добавление или удаление комнаты из выбранных при нажатии Ctrl
                            if (!selectedRooms.Contains(room))
                            {
                                selectedRooms.Add(room);
                                btnCancelSelection.Visible = selectedRooms.Count > 0;
                                UpdateStatus($"Добавлена к выбору: {room.Name}");
                            }
                            else
                            {
                                selectedRooms.Remove(room);
                                btnCancelSelection.Visible = selectedRooms.Count > 0;
                                UpdateStatus($"Удалена из выбора: {room.Name}");
                            }
                            btnCancelSelection.Visible = selectedRooms.Count > 0;
                        }
                        else
                        {
                            // Очистка предыдущего выбора и выбор текущей комнаты
                            selectedRooms.Clear();
                            selectedRooms.Add(room);
                            btnCancelSelection.Visible = selectedRooms.Count > 0;
                            UpdateStatus($"Выбрана комната: {room.Name}");
                            btnCancelSelection.Visible = true;
                        }
                        pnlMap.Invalidate();

                        // Начало перемещения выбранных комнат
                        selectedRoom = room;
                        lastMouseMapPosition = TransformPoint(new Point(e.X, e.Y));
                        isDragging = true;
                        break;
                    }
                }
            }
        }
    }

    private void PnlMap_MouseMove(object sender, MouseEventArgs e)
    {
        if (isEditMode)
        {
            if (isDragging && selectedRoom != null && pnlMap.Tag as string != "selecting_connection" && pnlMap.Tag as string != "deleting_connection")
            {
                // Преобразование текущей позиции курсора в координаты карты
                Point currentMapPoint = TransformPoint(new Point(e.X, e.Y));

                // Вычисление смещения
                int deltaX = currentMapPoint.X - lastMouseMapPosition.X;
                int deltaY = currentMapPoint.Y - lastMouseMapPosition.Y;

                // Применение смещения ко всем выбранным комнатам
                foreach (var room in selectedRooms)
                {
                    room.Coordinates = new Point(room.Coordinates.X + deltaX, room.Coordinates.Y + deltaY);
                }

                // Обновление последней позиции курсора
                lastMouseMapPosition = currentMapPoint;

                // Перерисовка панели
                pnlMap.Invalidate();
            }
            else if (pnlMap.Tag as string == "selecting_connection")
            {
                // Дополнительная логика при выборе соединений
            }
            else if (pnlMap.Tag as string == "deleting_connection")
            {
                // Дополнительная логика при удалении соединений
            }
        }
        else
        {
            if (isDragging)
            {
                mapOffset.X += e.X - lastMousePosition.X;
                mapOffset.Y += e.Y - lastMousePosition.Y;
                lastMousePosition = e.Location;
                pnlMap.Invalidate();
            }

            // Обновление статуса при наведении на комнату
            bool found = false;
            foreach (var room in floors[currentFloor].Rooms.Values)
            {
                var rect = GetRoomRectangle(room);
                if (rect.Contains(e.Location))
                {
                    UpdateStatus(room.Name);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                UpdateStatus(string.Empty);
            }
        }
    }

    private void PnlMap_MouseUp(object sender, MouseEventArgs e)
    {
        if (isEditMode)
        {
            isDragging = false;
        }
        else
        {
            if (e.Button == MouseButtons.Left)
                isDragging = false;
            else if (e.Button == MouseButtons.Right)
                isRightButtonDown = false;
        }
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

    private void BtnToggleConnections_Click(object sender, EventArgs e)
    {
        showConnections = !showConnections;
        pnlMap.Invalidate();
    }
    private void BtnEditMode_Click(object sender, EventArgs e)
    {
        isEditMode = !isEditMode;
        btnSave.Visible = isEditMode;
        btnAddConnection.Visible = isEditMode;
        btnDeleteConnection.Visible = isEditMode;
        btnAddRoom.Visible = isEditMode;
        btnCancelSelection.Visible = isEditMode; // Добавлено
        btnEditMode.Text = isEditMode ? "Выйти из редактирования" : "Редактировать";
        if (!isEditMode)
        {
            selectedRooms.Clear();
            pnlMap.Invalidate();
            UpdateStatus("Редактирование завершено.");
        }
    }


    private void BtnSave_Click(object sender, EventArgs e)
    {
        var building = new Building
        {
            Floors = floors.Values.ToList()
        };
        editorService.SaveBuilding(building);
        MessageBox.Show("Изменения сохранены.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        isEditMode = false;
        btnSave.Visible = false;
        btnAddConnection.Visible = false;
        btnDeleteConnection.Visible = false;
        btnEditMode.Text = "Редактировать";
        pnlMap.Invalidate();
    }

    private void BtnAddConnection_Click(object sender, EventArgs e)
    {
        UpdateStatus("Выберите две комнаты для соединения.");
        pnlMap.Tag = "selecting_connection";
    }

    private void BtnDeleteConnection_Click(object sender, EventArgs e)
    {
        UpdateStatus("Выберите соединение для удаления.");
        pnlMap.Tag = "deleting_connection";
    }

    private void BtnAddRoom_Click(object sender, EventArgs e)
    {
        // Сохранение позиции курсора относительно панели карты
        addRoomPosition = pnlMap.PointToClient(Cursor.Position);
        var pnlAddRoom = (sender as Button)?.Tag as Panel;
        if (pnlAddRoom != null)
        {
            pnlAddRoom.Visible = true;
            UpdateStatus("Введите название новой комнаты.");
        }
    }

    private void AddNewRoom(string roomName, Panel pnlAddRoom)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            UpdateStatus("Название кабинета не может быть пустым.");
            return;
        }

        var floor = floors[currentFloor];
        if (floor.Rooms.ContainsKey(roomName))
        {
            UpdateStatus("Кабинет с таким названием уже существует.");
            return;
        }

        // Расчет координат независимо от зума
        System.Drawing.Drawing2D.Matrix inverseMatrix = new System.Drawing.Drawing2D.Matrix();
        inverseMatrix.Rotate(-rotationAngle);
        inverseMatrix.Scale(1 / zoom, 1 / zoom);
        inverseMatrix.Translate(-mapOffset.X, -mapOffset.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
        Point[] pts = { addRoomPosition };
        inverseMatrix.TransformPoints(pts);
        Point transformedPoint = pts[0];

        // Добавление новой комнаты рядом с курсором
        Room newRoom = new Room
        {
            Name = roomName,
            FloorId = currentFloor,
            Coordinates = transformedPoint
        };
        floor.Rooms.Add(roomName, newRoom);
        pnlAddRoom.Visible = false;
        UpdateRoomSelectors();
        pnlMap.Invalidate();
        UpdateStatus($"Кабинет '{roomName}' добавлен.");
    }

    private void UpdateStatus(string message)
    {
        if (this.Tag is ToolStripStatusLabel statusLabel)
        {
            statusLabel.Text = message;
        }
    }

    private (string, string)? GetConnectionAtPoint(Point clickPoint)
    {
        var floor = floors[currentFloor];
        foreach (var connection in floor.Connections)
        {
            var fromRoom = GetRoomByName(connection.Item1, out _);
            var toRoom = GetRoomByName(connection.Item2, out _);
            if (fromRoom != null && toRoom != null)
            {
                var lineStart = TransformPoint(fromRoom.Coordinates);
                var lineEnd = TransformPoint(toRoom.Coordinates);
                if (IsPointNearLine(clickPoint, lineStart, lineEnd, 5))
                {
                    return connection;
                }
            }
        }
        return null;
    }

    private bool IsPointNearLine(Point p, Point p1, Point p2, double tolerance)
    {
        double distance = DistancePointToLine(p, p1, p2);
        return distance <= tolerance;
    }

    private double DistancePointToLine(Point p, Point p1, Point p2)
    {
        double A = p.X - p1.X;
        double B = p.Y - p1.Y;
        double C = p2.X - p1.X;
        double D = p2.Y - p1.Y;

        double dot = A * C + B * D;
        double len_sq = C * C + D * D;
        double param = -1;
        if (len_sq != 0) // in case of 0 length line
            param = dot / len_sq;

        double xx, yy;

        if (param < 0)
        {
            xx = p1.X;
            yy = p1.Y;
        }
        else if (param > 1)
        {
            xx = p2.X;
            yy = p2.Y;
        }
        else
        {
            xx = p1.X + param * C;
            yy = p1.Y + param * D;
        }

        double dx = p.X - xx;
        double dy = p.Y - yy;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    private Point TransformPoint(Point p)
    {
        System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
        matrix.Translate(mapOffset.X, mapOffset.Y);
        matrix.Scale(zoom, zoom);
        matrix.Rotate(rotationAngle);
        Point[] pts = { p };
        matrix.TransformPoints(pts);
        return pts[0];
    }
    private List<Room> selectedRooms = new List<Room>();

    private void SelectRoomForConnection(Point location)
    {
        foreach (var room in floors[currentFloor].Rooms.Values)
        {
            var rect = GetRoomRectangle(room);
            if (rect.Contains(location))
            {
                if (!selectedRooms.Contains(room))
                {
                    selectedRooms.Add(room);
                    btnCancelSelection.Visible = selectedRooms.Count > 0;
                    UpdateStatus($"Выбран кабинет для соединения: {room.Name}");
                    pnlMap.Invalidate();

                    if (selectedRooms.Count >= 2)
                    {
                        // Соединяем все выбранные комнаты попарно
                        for (int i = 0; i < selectedRooms.Count; i++)
                        {
                            for (int j = i + 1; j < selectedRooms.Count; j++)
                            {
                                var room1 = selectedRooms[i];
                                var room2 = selectedRooms[j];
                                if (!floors[currentFloor].Connections.Contains((room1.Name, room2.Name)))
                                {
                                    floors[currentFloor].Connections.Add((room1.Name, room2.Name));
                                    floors[currentFloor].Connections.Add((room2.Name, room1.Name));
                                    UpdateStatus($"Соединение добавлено между {room1.Name} и {room2.Name}.");
                                }
                            }
                        }
                        selectedRooms.Clear();
                        pnlMap.Tag = null;
                        pnlMap.Invalidate();
                    }
                }
                break;
            }
        }
    }
    private void SelectMultipleRooms(Point location)
    {
        foreach (var room in floors[currentFloor].Rooms.Values)
        {
            var rect = GetRoomRectangle(room);
            if (rect.Contains(location))
            {
                if (!selectedRooms.Contains(room))
                {
                    selectedRooms.Add(room);
                    btnCancelSelection.Visible = true;
                    UpdateStatus($"Добавлена к выбору: {room.Name}");
                }
                else
                {
                    selectedRooms.Remove(room);
                    UpdateStatus($"Удалена из выбора: {room.Name}");
                }
                pnlMap.Invalidate();
                break;
            }
        }
    }

    private void BtnCancelSelection_Click(object sender, EventArgs e)
    {
        selectedRooms.Clear();
        selectedRoom = null;
        pnlMap.Tag = null;
        pnlMap.Invalidate();
        UpdateStatus("Выбор отменен.");
        btnCancelSelection.Visible = false;
    }

    private void DeleteConnectionAtPoint(Point location)
    {
        foreach (var room in floors[currentFloor].Rooms.Values)
        {
            var rect = GetRoomRectangle(room);
            if (rect.Contains(location))
            {
                if (!selectedRooms.Contains(room))
                {
                    selectedRooms.Add(room);
                    UpdateStatus($"Выбран кабинет для удаления соединения: {room.Name}");
                    pnlMap.Invalidate();

                    if (selectedRooms.Count >= 2)
                    {
                        // Удаляем соединения между всеми выбранными комнатами
                        for (int i = 0; i < selectedRooms.Count; i++)
                        {
                            for (int j = i + 1; j < selectedRooms.Count; j++)
                            {
                                var room1 = selectedRooms[i];
                                var room2 = selectedRooms[j];
                                if (floors[currentFloor].Connections.Contains((room1.Name, room2.Name)))
                                {
                                    floors[currentFloor].Connections.Remove((room1.Name, room2.Name));
                                    floors[currentFloor].Connections.Remove((room2.Name, room1.Name));
                                    UpdateStatus($"Соединение удалено между {room1.Name} и {room2.Name}.");
                                }
                            }
                        }
                        selectedRooms.Clear();
                        pnlMap.Tag = null;
                        pnlMap.Invalidate();
                    }
                }
                break;
            }
        }
    }

    private Rectangle GetRoomRectangle(Room room)
    {
        var transformedPoint = TransformPoint(room.Coordinates);
        return new Rectangle(
            (int)(transformedPoint.X - 10 * zoom),
            (int)(transformedPoint.Y - 10 * zoom),
            (int)(20 * zoom),
            (int)(20 * zoom));
    }

    private Point GetTransformedCoordinates()
    {
        // Получение координат курсора относительно панели карты
        Point mousePos = pnlMap.PointToClient(Cursor.Position);
        System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
        matrix.Translate(mapOffset.X, mapOffset.Y);
        matrix.Scale(zoom, zoom);
        matrix.Rotate(rotationAngle);
        matrix.Invert();
        Point[] pts = { mousePos };
        matrix.TransformPoints(pts);
        return pts[0];
    }
    public class CustomPanel : Panel
    {
        public CustomPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }
    }
}