using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class Schedule
    {
        public Dictionary<int, List<int>> DoctorToPatients { get; set; } = new Dictionary<int, List<int>>(); // Doctor ID to list of patient IDs
        public Dictionary<int, int> PatientToDoctor { get; set; } = new Dictionary<int, int>(); // Patient ID to Doctor ID
        public Dictionary<DateTime, Dictionary<int, List<int>>> SurgerySchedule { get; set; } = new Dictionary<DateTime, Dictionary<int, List<int>>>(); // Date -> OperatingRoom -> List of PatientIDs

        // Get all patients assigned to a specific doctor
        public List<int> GetPatientsForDoctor(int doctorId)
        {
            return DoctorToPatients.ContainsKey(doctorId) ? DoctorToPatients[doctorId] : new List<int>();
        }

        // Get the doctor assigned to a specific patient
        public int? GetDoctorForPatient(int patientId)
        {
            return PatientToDoctor.ContainsKey(patientId) ? PatientToDoctor[patientId] : (int?)null;
        }

        // Add a patient-doctor assignment
        public void AssignPatientToDoctor(int patientId, int doctorId)
        {
            // Remove existing assignment if any
            if (PatientToDoctor.ContainsKey(patientId))
            {
                int currentDoctorId = PatientToDoctor[patientId];
                if (DoctorToPatients.ContainsKey(currentDoctorId))
                {
                    DoctorToPatients[currentDoctorId].Remove(patientId);
                }
            }

            // Add new assignment
            PatientToDoctor[patientId] = doctorId;

            if (!DoctorToPatients.ContainsKey(doctorId))
            {
                DoctorToPatients[doctorId] = new List<int>();
            }

            DoctorToPatients[doctorId].Add(patientId);
        }

        // Add a surgery assignment
        public bool ScheduleSurgery(int patientId, int surgeonId, int operatingRoomId, DateTime surgeryDate)
        {
            // Initialize dictionaries if needed
            if (!SurgerySchedule.ContainsKey(surgeryDate))
            {
                SurgerySchedule[surgeryDate] = new Dictionary<int, List<int>>();
            }

            if (!SurgerySchedule[surgeryDate].ContainsKey(operatingRoomId))
            {
                SurgerySchedule[surgeryDate][operatingRoomId] = new List<int>();
            }

            // Check if the operating room is available
            if (SurgerySchedule[surgeryDate][operatingRoomId].Count > 0)
            {
                return false; // Room already booked
            }

            // Schedule the surgery
            SurgerySchedule[surgeryDate][operatingRoomId].Add(patientId);
            return true;
        }

        // Calculate current schedule fitness score
        public double CalculateFitnessScore(List<Doctor> doctors, List<Patient> patients)
        {
            double score = 0;

            // Score for patient assignments
            foreach (var patientId in PatientToDoctor.Keys)
            {
                var doctorId = PatientToDoctor[patientId];
                var patient = patients.FirstOrDefault(p => p.Id == patientId);
                var doctor = doctors.FirstOrDefault(d => d.Id == doctorId);

                if (patient != null && doctor != null)
                {
                    // Specialization match
                    if (doctor.Specialization == patient.RequiredSpecialization)
                    {
                        score += 50;
                    }

                    // Urgency handling
                    score += patient.GetUrgencyValue() * 10;

                    // Continuity of care
                    if (patient.HasContinuityOfCare(doctorId))
                    {
                        score += 15;
                    }
                }
            }

            // Score for workload balance
            var doctorWorkloads = doctors.Select(d => d.WorkloadPercentage).ToList();
            if (doctorWorkloads.Any())
            {
                double workloadVariance = doctorWorkloads.Select(w => Math.Pow(w - doctorWorkloads.Average(), 2)).Average();
                score -= workloadVariance * 0.5; // Penalize uneven workload distribution
            }

            return score;
        }
    }
}