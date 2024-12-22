using INStructed.Models;
using INStructed.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class MainForm : Form
{
    private ComboBox cmbFloors;
    private ComboBox cmbStartRoom;
    private ComboBox cmbEndRoom;
    private Panel pnlMap;
    private Dictionary<int, Floor> floors;
    private int currentFloor;
    private List<string> currentRoute;

    public MainForm()
    {
        InitializeComponent();
        InitializeFloors();
    }

    private void InitializeComponent()
    {
        // Выбор этажа
        cmbFloors = new ComboBox
        {
            Location = new Point(20, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200
        };
        cmbFloors.SelectedIndexChanged += (s, e) =>
        {
            currentFloor = cmbFloors.SelectedIndex;
            UpdateRoomSelectors();
            pnlMap.Invalidate();
        };

        // Выбор начальной комнаты
        cmbStartRoom = new ComboBox
        {
            Location = new Point(250, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };

        // Выбор конечной комнаты
        cmbEndRoom = new ComboBox
        {
            Location = new Point(420, 20),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };

        // Панель карты
        pnlMap = new Panel
        {
            Location = new Point(20, 60),
            Size = new Size(800, 600),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };
        pnlMap.Paint += PnlMap_Paint;

        // Добавление кнопки "Построить маршрут"
        var btnFindRoute = new Button
        {
            Text = "Построить маршрут",
            Location = new Point(590, 20),
            Width = 150
        };
        btnFindRoute.Click += BtnFindRoute_Click;

        // Добавление элементов на форму
        Controls.Add(cmbFloors);
        Controls.Add(cmbStartRoom);
        Controls.Add(cmbEndRoom);
        Controls.Add(btnFindRoute);
        Controls.Add(pnlMap);

        // Настройка формы
        Text = "Навигация внутри здания";
        ClientSize = new Size(860, 700);
    }

    private void InitializeFloors()
    {
        // Создание этажей с помещениями
        floors = new Dictionary<int, Floor>
        {
            { 0, new Floor(0, "Первый этаж", new Dictionary<string, Room>
                {
                    { "Вход", new Room("Вход", new Point(50, 500)) },
                    { "Гардероб", new Room("Гардероб", new Point(100, 450)) },
                    { "Кабинет 1", new Room("Кабинет 1", new Point(150, 400)) },
                    { "Кабинет 2", new Room("Кабинет 2", new Point(300, 350)) },
                    { "Кабинет 3", new Room("Кабинет 3", new Point(300, 200)) },
                    { "Кабинет 4", new Room("Кабинет 4", new Point(200, 150)) },
                    { "Деканат", new Room("Деканат", new Point(100, 250)) },
                    { "Лестница1", new Room("Лестница1", new Point(50, 200)) },
                    { "Лестница2", new Room("Лестница2", new Point(400, 200)) },
                    { "Туалет", new Room("Туалет", new Point(500, 100)) },
                    { "Точка кипения (лекционная)", new Room("Точка кипения (лекционная)", new Point(600, 50)) }
                },
                new List<(string, string)>
                {
                    ("Вход", "Гардероб"),
                    ("Гардероб", "Кабинет 1"),
                    ("Кабинет 1", "Кабинет 2"),
                    ("Кабинет 2", "Кабинет 3"),
                    ("Кабинет 3", "Кабинет 4"),
                    ("Кабинет 4", "Деканат"),
                    ("Деканат", "Лестница1"),
                    ("Кабинет 2", "Лестница2"),
                    ("Лестница2", "Туалет"),
                    ("Туалет", "Точка кипения (лекционная)")
                })
            },
            { 1, new Floor(1, "Второй этаж", new Dictionary<string, Room>
                 {
                    { "Лестница1", new Room("Лестница1", new Point(50, 500)) },
                    { "Кабинет 201", new Room("Кабинет 201", new Point(150, 450)) },
                    { "Кабинет 202", new Room("Кабинет 202", new Point(250, 400)) },
                    { "Кабинет 203", new Room("Кабинет 203", new Point(350, 350)) },
                    { "Кабинет 204", new Room("Кабинет 204", new Point(450, 350)) },
                    { "Завкафедры", new Room("Завкафедры", new Point(550, 300)) },
                    { "Кабинет 205", new Room("Кабинет 205", new Point(650, 250)) },
                    { "Кабинет 206", new Room("Кабинет 206", new Point(750, 200)) },
                    { "Туалет", new Room("Туалет", new Point(850, 150)) },
                    { "Кабинет 217", new Room("Кабинет 217", new Point(950, 100)) },
                    { "Кабинет 218", new Room("Кабинет 218", new Point(1050, 50)) },
                    { "Лестница2", new Room("Лестница2", new Point(1150, 500)) }
                },
                new List<(string, string)>
                {
                    ("Лестница1", "Кабинет 201"),
                    ("Кабинет 201", "Кабинет 202"),
                    ("Кабинет 202", "Кабинет 203"),
                    ("Кабинет 203", "Кабинет 204"),
                    ("Кабинет 204", "Завкафедры"),
                    ("Завкафедры", "Кабинет 205"),
                    ("Кабинет 205", "Кабинет 206"),
                    ("Кабинет 206", "Туалет"),
                    ("Туалет", "Кабинет 217"),
                    ("Кабинет 217", "Кабинет 218"),
                    ("Кабинет 218", "Лестница2")
                })
        }

    };
        

        // Заполнение ComboBox для этажей
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
        var endRoom = cmbEndRoom.SelectedItem as string;

        if (startRoom == null || endRoom == null)
        {
            MessageBox.Show("Пожалуйста, выберите начальную и конечную комнаты.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Находим маршрут на текущем этаже
        var routeFinder = new RouteFinder(BuildGraphForFloor(currentFloor));
        currentRoute = routeFinder.FindShortestPath(startRoom, endRoom);

        pnlMap.Invalidate();
    }

    private Dictionary<string, List<(string, int)>> BuildGraphForFloor(int floorId)
    {
        var floor = floors[floorId];
        var graph = new Dictionary<string, List<(string, int)>>();

        foreach (var connection in floor.Connections)
        {
            if (!graph.ContainsKey(connection.Item1))
                graph[connection.Item1] = new List<(string, int)>();

            if (!graph.ContainsKey(connection.Item2))
                graph[connection.Item2] = new List<(string, int)>();

            graph[connection.Item1].Add((connection.Item2, 1));
            graph[connection.Item2].Add((connection.Item1, 1));
        }

        return graph;
    }

    private void PnlMap_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var floor = floors[currentFloor];

        // Отображение помещений
        foreach (var room in floor.Rooms.Values)
        {
            g.FillEllipse(Brushes.LightBlue, room.Coordinates.X - 10, room.Coordinates.Y - 10, 20, 20);
            g.DrawString(room.Name, this.Font, Brushes.Black, room.Coordinates.X - 20, room.Coordinates.Y - 30);
        }

        // Отображение связей
        foreach (var connection in floor.Connections)
        {
            var start = floor.Rooms[connection.Item1].Coordinates;
            var end = floor.Rooms[connection.Item2].Coordinates;
            g.DrawLine(Pens.Black, start, end);
        }

        // Отображение маршрута
        if (currentRoute != null && currentRoute.Count > 1)
        {
            using (var pen = new Pen(Color.Red, 3)) // Жирная красная линия
            {
                for (int i = 0; i < currentRoute.Count - 1; i++)
                {
                    var start = floor.Rooms[currentRoute[i]].Coordinates;
                    var end = floor.Rooms[currentRoute[i + 1]].Coordinates;
                    g.DrawLine(pen, start, end);
                }
            }
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
