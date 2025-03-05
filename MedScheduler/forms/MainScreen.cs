using MedScheduler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MedScheduler
{
    public partial class MainScreen : Form
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        private PanelNavigationManager navigationManager;
        private DataManager db = new DataManager();
        private Timer movementTimer;
        private Timer disappearTimer;
        private double angle = 0;
        private int centerX;
        private int centerY;
        private int radius = 200;
        public MainScreen()
        {
            navigationManager = new PanelNavigationManager(this);
            this.Size = new Size(1920, 1080);

            InitializeComponent();
            navigationManager.RegisterScreen("DataCenter", DataCenter);
            //AllocConsole();
            //// Expanded doctor list
            
        }

        private void MoveInCircle()
        {
            // Initialize timers
            movementTimer = new Timer();
            movementTimer.Interval = 20; // 20 milliseconds for smooth animation
            movementTimer.Tick += CircularMovement;

            disappearTimer = new Timer();
            disappearTimer.Interval = 5000; // 5 seconds
            disappearTimer.Tick += DisappearDoctor;

            // Calculate center point (you can adjust these as needed)
            centerX = this.ClientSize.Width / 2;
            centerY = this.ClientSize.Height / 2;

            // Start the movement
            movementTimer.Start();
            disappearTimer.Start();
        }

        private void CircularMovement(object sender, EventArgs e)
        {
            // Calculate new position using parametric equations of a circle
            int x = (int)(centerX + radius * Math.Cos(angle));
            int y = (int)(centerY + radius * Math.Sin(angle));

            // Move the PictureBox
            doctor.Location = new Point(x, y);

            // Increment angle (adjust speed by changing the increment)
            angle += 0.1; // Radians per tick

            // Reset angle to prevent potential overflow
            if (angle >= 2 * Math.PI)
                angle = 0;
        }

        private void DisappearDoctor(object sender, EventArgs e)
        {
            // Stop both timers
            movementTimer.Stop();
            disappearTimer.Stop();

            // Make the PictureBox invisible
            doctor.Visible = false;
        }

        // Example of how to call the method (you would typically call this from a button click or form load)
        private void StartDoctorMovement()
        {
            MoveInCircle();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            navigationManager.NavigateTo("DataCenter");
        }

        private void DataCenter_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void modernButton1_Click(object sender, EventArgs e)
        {
            var doctors = db.GetDoctors();

            modernButton1.Visible = false;
            StartDoctorMovement();
            var patients = db.GetPatients();

            var genetics = new Genetics(100, doctors, patients);
            var bestSchedule = genetics.Solve();

            //Output the best schedule
            foreach (var doctorId in bestSchedule.DoctorToPatients.Keys)
            {
                Console.WriteLine($"Doctor {doctorId} is assigned to patients: {string.Join(", ", bestSchedule.DoctorToPatients[doctorId])}");
            }
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
