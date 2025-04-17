using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace MedScheduler
{
    public class DataManager
    {
        private List<Doctor> doctors;
        private List<Patient> patients;
        private List<MedicalProcedure> procedures;
        private List<OperatingRoom> operatingRooms;
        private Schedule currentSchedule;

        public DataManager()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            // Create data generator
            DataGenerator generator = new DataGenerator();

            // Generate doctors (50)
            doctors = generator.GenerateDoctors(200);

            // Generate patients (100) - doctors are needed for previous doctor history
            patients = generator.GeneratePatients(500
                , doctors);

            // Generate medical procedures (20)
            procedures = generator.GenerateProcedures(30);

            // Generate operating rooms (10)
            operatingRooms = generator.GenerateOperatingRooms(20);

            // Generate an initial schedule
            currentSchedule = generator.GenerateInitialSchedule(doctors, patients);

            Console.WriteLine($"Generated {doctors.Count} doctors");
            Console.WriteLine($"Generated {patients.Count} patients");
            Console.WriteLine($"Generated {procedures.Count} medical procedures");
            Console.WriteLine($"Generated {operatingRooms.Count} operating rooms");
            Console.WriteLine($"Generated initial schedule with {currentSchedule.PatientToDoctor.Count} doctor-patient assignments");
        }

        // Existing methods
        public List<Doctor> GetDoctors()
        {
            return doctors;
        }

        public List<Patient> GetPatients()
        {
            return patients;
        }

        public List<MedicalProcedure> GetProcedures()
        {
            return procedures;
        }

        public List<OperatingRoom> GetOperatingRooms()
        {
            return operatingRooms;
        }

        public Schedule GetCurrentSchedule()
        {
            return currentSchedule;
        }

        // Method to create sample statistics for the dashboard

    }
}