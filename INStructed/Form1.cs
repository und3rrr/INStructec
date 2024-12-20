using INStructed.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class MainForm : Form
{
    private ComboBox cmbFloors;
    private Panel pnlMap;
    private Dictionary<int, Floor> floors;
    private int currentFloor;

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
            pnlMap.Invalidate();
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

        // Добавление элементов на форму
        Controls.Add(cmbFloors);
        Controls.Add(pnlMap);

        // Настройка формы
        Text = "Indoor Navigation";
        ClientSize = new Size(860, 700);
    }

    private void InitializeFloors()
    {
        // Создание этажей с помещениями
        floors = new Dictionary<int, Floor>
        {
            { 0, new Floor(0, "Первый этаж", new Dictionary<string, Room>
                {
                    { "Room1", new Room("Room1", new Point(100, 100)) },
                    { "Room2", new Room("Room2", new Point(300, 100)) },
                    { "Stairs", new Room("Stairs", new Point(200, 200)) }
                },
                new List<(string, string)>
                {
                    ("Room1", "Room2"),
                    ("Room2", "Stairs"),
                    ("Stairs", "Room1")
                })
            },
            { 1, new Floor(1, "Второй этаж", new Dictionary<string, Room>
                {
                    { "Room3", new Room("Room3", new Point(100, 100)) },
                    { "Room4", new Room("Room4", new Point(300, 100)) },
                    { "Elevator", new Room("Elevator", new Point(200, 200)) }
                },
                new List<(string, string)>
                {
                    ("Room3", "Room4"),
                    ("Room4", "Elevator"),
                    ("Elevator", "Room3")
                })
            }
        };

        // Заполнение ComboBox
        foreach (var floor in floors.Values)
        {
            cmbFloors.Items.Add(floor.Name);
        }
        cmbFloors.SelectedIndex = 0;
        currentFloor = 0;
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
    }
    [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
}