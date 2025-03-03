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
        public MainScreen()
        {
            InitializeComponent();

            AllocConsole();
            // Expanded doctor list
            var doctors = new List<Doctor>
{
    // Original doctors (1-18)
    new Doctor { Id = 1, Specialization = "Cardiology", MaxWorkload = 10 },
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
    new Doctor { Id = 18, Specialization = "Oncology", MaxWorkload = 10 },
    
    // New doctors (19-50)
    new Doctor { Id = 19, Specialization = "Emergency Medicine", MaxWorkload = 14 },
    new Doctor { Id = 20, Specialization = "Emergency Medicine", MaxWorkload = 14 },
    new Doctor { Id = 21, Specialization = "Emergency Medicine", MaxWorkload = 14 },
    new Doctor { Id = 22, Specialization = "Pulmonology", MaxWorkload = 10 },
    new Doctor { Id = 23, Specialization = "Pulmonology", MaxWorkload = 10 },
    new Doctor { Id = 24, Specialization = "Gastroenterology", MaxWorkload = 9 },
    new Doctor { Id = 25, Specialization = "Gastroenterology", MaxWorkload = 9 },
    new Doctor { Id = 26, Specialization = "Endocrinology", MaxWorkload = 12 },
    new Doctor { Id = 27, Specialization = "Endocrinology", MaxWorkload = 12 },
    new Doctor { Id = 28, Specialization = "Rheumatology", MaxWorkload = 8 },
    new Doctor { Id = 29, Specialization = "Rheumatology", MaxWorkload = 8 },
    new Doctor { Id = 30, Specialization = "Ophthalmology", MaxWorkload = 12 },
    new Doctor { Id = 31, Specialization = "Ophthalmology", MaxWorkload = 12 },
    new Doctor { Id = 32, Specialization = "Psychiatry", MaxWorkload = 15 },
    new Doctor { Id = 33, Specialization = "Psychiatry", MaxWorkload = 15 },
    new Doctor { Id = 34, Specialization = "Urology", MaxWorkload = 9 },
    new Doctor { Id = 35, Specialization = "Urology", MaxWorkload = 9 },
    new Doctor { Id = 36, Specialization = "Nephrology", MaxWorkload = 11 },
    new Doctor { Id = 37, Specialization = "Nephrology", MaxWorkload = 11 },
    new Doctor { Id = 38, Specialization = "Cardiology", MaxWorkload = 10 },
    new Doctor { Id = 39, Specialization = "Orthopedics", MaxWorkload = 8 },
    new Doctor { Id = 40, Specialization = "Neurology", MaxWorkload = 12 },
    new Doctor { Id = 41, Specialization = "Pediatrics", MaxWorkload = 15 },
    new Doctor { Id = 42, Specialization = "Dermatology", MaxWorkload = 10 },
    new Doctor { Id = 43, Specialization = "Oncology", MaxWorkload = 10 },
    new Doctor { Id = 44, Specialization = "Geriatrics", MaxWorkload = 12 },
    new Doctor { Id = 45, Specialization = "Geriatrics", MaxWorkload = 12 },
    new Doctor { Id = 46, Specialization = "Infectious Disease", MaxWorkload = 10 },
    new Doctor { Id = 47, Specialization = "Infectious Disease", MaxWorkload = 10 },
    new Doctor { Id = 48, Specialization = "Hematology", MaxWorkload = 9 },
    new Doctor { Id = 49, Specialization = "Hematology", MaxWorkload = 9 },
    new Doctor { Id = 50, Specialization = "OB/GYN", MaxWorkload = 14 }
};

            // Expanded patient list
            var patients = new List<Patient>
{
    // Original patients (1-30)
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
    new Patient { Id = 30, Condition = "Multiple Sclerosis", Urgency = "High", RequiredSpecialization = "Neurology" },
    
    // New patients (31-100)
    new Patient { Id = 31, Condition = "Chest Pain", Urgency = "High", RequiredSpecialization = "Emergency Medicine" },
    new Patient { Id = 32, Condition = "Severe Allergic Reaction", Urgency = "High", RequiredSpecialization = "Emergency Medicine" },
    new Patient { Id = 33, Condition = "Trauma from Car Accident", Urgency = "High", RequiredSpecialization = "Emergency Medicine" },
    new Patient { Id = 34, Condition = "COPD", Urgency = "Medium", RequiredSpecialization = "Pulmonology" },
    new Patient { Id = 35, Condition = "Pulmonary Fibrosis", Urgency = "High", RequiredSpecialization = "Pulmonology" },
    new Patient { Id = 36, Condition = "Sleep Apnea", Urgency = "Low", RequiredSpecialization = "Pulmonology" },
    new Patient { Id = 37, Condition = "Acid Reflux (GERD)", Urgency = "Low", RequiredSpecialization = "Gastroenterology" },
    new Patient { Id = 38, Condition = "Ulcerative Colitis", Urgency = "Medium", RequiredSpecialization = "Gastroenterology" },
    new Patient { Id = 39, Condition = "Crohn's Disease", Urgency = "Medium", RequiredSpecialization = "Gastroenterology" },
    new Patient { Id = 40, Condition = "Type 1 Diabetes", Urgency = "Medium", RequiredSpecialization = "Endocrinology" },
    new Patient { Id = 41, Condition = "Hypothyroidism", Urgency = "Low", RequiredSpecialization = "Endocrinology" },
    new Patient { Id = 42, Condition = "Adrenal Insufficiency", Urgency = "High", RequiredSpecialization = "Endocrinology" },
    new Patient { Id = 43, Condition = "Rheumatoid Arthritis", Urgency = "Medium", RequiredSpecialization = "Rheumatology" },
    new Patient { Id = 44, Condition = "Lupus", Urgency = "High", RequiredSpecialization = "Rheumatology" },
    new Patient { Id = 45, Condition = "Gout", Urgency = "Medium", RequiredSpecialization = "Rheumatology" },
    new Patient { Id = 46, Condition = "Cataracts", Urgency = "Low", RequiredSpecialization = "Ophthalmology" },
    new Patient { Id = 47, Condition = "Glaucoma", Urgency = "Medium", RequiredSpecialization = "Ophthalmology" },
    new Patient { Id = 48, Condition = "Retinal Detachment", Urgency = "High", RequiredSpecialization = "Ophthalmology" },
    new Patient { Id = 49, Condition = "Major Depression", Urgency = "High", RequiredSpecialization = "Psychiatry" },
    new Patient { Id = 50, Condition = "Anxiety Disorder", Urgency = "Medium", RequiredSpecialization = "Psychiatry" },
    new Patient { Id = 51, Condition = "Bipolar Disorder", Urgency = "Medium", RequiredSpecialization = "Psychiatry" },
    new Patient { Id = 52, Condition = "Kidney Stones", Urgency = "High", RequiredSpecialization = "Urology" },
    new Patient { Id = 53, Condition = "Enlarged Prostate", Urgency = "Medium", RequiredSpecialization = "Urology" },
    new Patient { Id = 54, Condition = "Urinary Tract Infection", Urgency = "Low", RequiredSpecialization = "Urology" },
    new Patient { Id = 55, Condition = "Chronic Kidney Disease", Urgency = "High", RequiredSpecialization = "Nephrology" },
    new Patient { Id = 56, Condition = "Polycystic Kidney Disease", Urgency = "Medium", RequiredSpecialization = "Nephrology" },
    new Patient { Id = 57, Condition = "Acute Kidney Injury", Urgency = "High", RequiredSpecialization = "Nephrology" },
    new Patient { Id = 58, Condition = "Coronary Artery Disease", Urgency = "High", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 59, Condition = "Heart Valve Disease", Urgency = "Medium", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 60, Condition = "Congenital Heart Defect", Urgency = "High", RequiredSpecialization = "Cardiology" },
    new Patient { Id = 61, Condition = "Hip Replacement", Urgency = "Medium", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 62, Condition = "Scoliosis", Urgency = "Low", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 63, Condition = "Sports Injury", Urgency = "Medium", RequiredSpecialization = "Orthopedics" },
    new Patient { Id = 64, Condition = "Bell's Palsy", Urgency = "Medium", RequiredSpecialization = "Neurology" },
    new Patient { Id = 65, Condition = "Huntington's Disease", Urgency = "High", RequiredSpecialization = "Neurology" },
    new Patient { Id = 66, Condition = "Neuropathy", Urgency = "Medium", RequiredSpecialization = "Neurology" },
    new Patient { Id = 67, Condition = "Developmental Delay", Urgency = "Medium", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 68, Condition = "Measles", Urgency = "High", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 69, Condition = "Growth Disorder", Urgency = "Medium", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 70, Condition = "Skin Abscess", Urgency = "Medium", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 71, Condition = "Dermatitis", Urgency = "Low", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 72, Condition = "Fungal Infection", Urgency = "Low", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 73, Condition = "Breast Cancer", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 74, Condition = "Prostate Cancer", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 75, Condition = "Lung Cancer", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 76, Condition = "Dementia", Urgency = "Medium", RequiredSpecialization = "Geriatrics" },
    new Patient { Id = 77, Condition = "Mobility Issues", Urgency = "Medium", RequiredSpecialization = "Geriatrics" },
    new Patient { Id = 78, Condition = "Age-related Macular Degeneration", Urgency = "Medium", RequiredSpecialization = "Geriatrics" },
    new Patient { Id = 79, Condition = "HIV/AIDS", Urgency = "High", RequiredSpecialization = "Infectious Disease" },
    new Patient { Id = 80, Condition = "Tuberculosis", Urgency = "High", RequiredSpecialization = "Infectious Disease" },
    new Patient { Id = 81, Condition = "Hepatitis C", Urgency = "Medium", RequiredSpecialization = "Infectious Disease" },
    new Patient { Id = 82, Condition = "Anemia", Urgency = "Medium", RequiredSpecialization = "Hematology" },
    new Patient { Id = 83, Condition = "Hemophilia", Urgency = "High", RequiredSpecialization = "Hematology" },
    new Patient { Id = 84, Condition = "Thrombocytopenia", Urgency = "High", RequiredSpecialization = "Hematology" },
    new Patient { Id = 85, Condition = "Pregnancy Complications", Urgency = "High", RequiredSpecialization = "OB/GYN" },
    new Patient { Id = 86, Condition = "Endometriosis", Urgency = "Medium", RequiredSpecialization = "OB/GYN" },
    new Patient { Id = 87, Condition = "Pelvic Inflammatory Disease", Urgency = "Medium", RequiredSpecialization = "OB/GYN" },
    new Patient { Id = 88, Condition = "Asthma Attack", Urgency = "High", RequiredSpecialization = "Emergency Medicine" },
    new Patient { Id = 89, Condition = "Acute Bronchitis", Urgency = "Medium", RequiredSpecialization = "Pulmonology" },
    new Patient { Id = 90, Condition = "Peptic Ulcer", Urgency = "Medium", RequiredSpecialization = "Gastroenterology" },
    new Patient { Id = 91, Condition = "Type 2 Diabetes", Urgency = "Medium", RequiredSpecialization = "Endocrinology" },
    new Patient { Id = 92, Condition = "Fibromyalgia", Urgency = "Medium", RequiredSpecialization = "Rheumatology" },
    new Patient { Id = 93, Condition = "Macular Degeneration", Urgency = "Medium", RequiredSpecialization = "Ophthalmology" },
    new Patient { Id = 94, Condition = "Schizophrenia", Urgency = "High", RequiredSpecialization = "Psychiatry" },
    new Patient { Id = 95, Condition = "Bladder Cancer", Urgency = "High", RequiredSpecialization = "Urology" },
    new Patient { Id = 96, Condition = "Glomerulonephritis", Urgency = "High", RequiredSpecialization = "Nephrology" },
    new Patient { Id = 97, Condition = "Chicken Pox", Urgency = "Low", RequiredSpecialization = "Pediatrics" },
    new Patient { Id = 98, Condition = "Alopecia", Urgency = "Low", RequiredSpecialization = "Dermatology" },
    new Patient { Id = 99, Condition = "Pancreatic Cancer", Urgency = "High", RequiredSpecialization = "Oncology" },
    new Patient { Id = 100, Condition = "Pressure Ulcers", Urgency = "Medium", RequiredSpecialization = "Geriatrics" }
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataCenter d = new DataCenter();
            d.Show();
        }
    }
}
