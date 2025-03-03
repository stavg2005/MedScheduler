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
        private PanelNavigationManager navigationManager;
        private DataManager db = new DataManager();
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
        public MainScreen()
        {
            navigationManager = new PanelNavigationManager(this);
            
            
            InitializeComponent();
            navigationManager.RegisterScreen("DataCenter", DataCenter);
            //AllocConsole();
            //// Expanded doctor list
            
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


            //Expanded patient list
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
    }
}
