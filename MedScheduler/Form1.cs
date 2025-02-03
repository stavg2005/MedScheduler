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
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
        public Form1()
        {
            InitializeComponent();

            AllocConsole();
            var doctors = new List<Doctor>
{      new Doctor { Id = 1, Specialization = "Cardiology", MaxWorkload = 10 },
    new Doctor { Id = 2, Specialization = "Orthopedics", MaxWorkload = 8 },
    new Doctor { Id = 3, Specialization = "Neurology", MaxWorkload = 12 },
    new Doctor { Id = 4, Specialization = "Cardiology", MaxWorkload = 10 },
    new Doctor { Id = 5, Specialization = "Orthopedics", MaxWorkload = 8 },
    new Doctor { Id = 6, Specialization = "Neurology", MaxWorkload = 12 },
    new Doctor { Id = 7, Specialization = "Cardiology", MaxWorkload = 10 },
    new Doctor { Id = 8, Specialization = "Orthopedics", MaxWorkload = 8 },
    new Doctor { Id = 9, Specialization = "Neurology", MaxWorkload = 12 },
    new Doctor { Id = 10, Specialization = "Cardiology", MaxWorkload = 10 },
    new Doctor { Id = 11, Specialization = "Orthopedics", MaxWorkload = 8 },
    new Doctor { Id = 12, Specialization = "Neurology", MaxWorkload = 12 },
    new Doctor { Id = 13, Specialization = "Pediatrics", MaxWorkload = 15 },
    new Doctor { Id = 14, Specialization = "Dermatology", MaxWorkload = 10 },
    new Doctor { Id = 15, Specialization = "Oncology", MaxWorkload = 10 },
    new Doctor { Id = 16, Specialization = "Pediatrics", MaxWorkload = 15 },
    new Doctor { Id = 17, Specialization = "Dermatology", MaxWorkload = 10 },
    new Doctor { Id = 18, Specialization = "Oncology", MaxWorkload = 10 }
    };

                var patients = new List<Patient>
    {
           new Patient { Id = 1, Condition = "Heart Disease", Urgency = "High", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 2, Condition = "Broken Leg", Urgency = "Medium", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 3, Condition = "Migraine", Urgency = "Low", RequiredSpecialization = "Neurology" },
    new Patient { Id = 4, Condition = "Arrhythmia", Urgency = "High", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 5, Condition = "Fractured Arm", Urgency = "Medium", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 6, Condition = "Epilepsy", Urgency = "High", RequiredSpecialization = "Neurology" },
    new Patient { Id = 7, Condition = "Hypertension", Urgency = "Medium", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 8, Condition = "Torn Ligament", Urgency = "Low", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 9, Condition = "Parkinson's", Urgency = "High", RequiredSpecialization = "Neurology" },
    new Patient { Id = 10, Condition = "Heart Attack", Urgency = "High", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 11, Condition = "Spinal Injury", Urgency = "High", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 12, Condition = "Alzheimer's", Urgency = "Medium", RequiredSpecialization = "Neurology" },
    new Patient { Id = 13, Condition = "Common Cold", Urgency = "Low", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 14, Condition = "Eczema", Urgency = "Medium", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 15, Condition = "Skin Cancer", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 16, Condition = "Flu", Urgency = "Low", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 17, Condition = "Psoriasis", Urgency = "Medium", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 18, Condition = "Leukemia", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 19, Condition = "Asthma", Urgency = "Medium", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 20, Condition = "Acne", Urgency = "Low", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 21, Condition = "Lymphoma", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 22, Condition = "Pneumonia", Urgency = "High", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 23, Condition = "Rosacea", Urgency = "Medium", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 24, Condition = "Melanoma", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 25, Condition = "Bronchitis", Urgency = "Medium", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 26, Condition = "Hives", Urgency = "Low", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 27, Condition = "Brain Tumor", Urgency = "High", RequiredSpecialization = "Neurology" },
    new Patient { Id = 28, Condition = "Stroke", Urgency = "High", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 29, Condition = "Osteoporosis", Urgency = "Medium", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 30, Condition = "Multiple Sclerosis", Urgency = "High", RequiredSpecialization = "Neurology" }
    };

            var genetics = new Genetics(100, doctors, patients);
            var bestSchedule = genetics.Solve();

            // Output the best schedule
            foreach (var doctorId in bestSchedule.DoctorToPatients.Keys)
            {
                Console.WriteLine($"Doctor {doctorId} is assigned to patients: {string.Join(", ", bestSchedule.DoctorToPatients[doctorId])}");
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
