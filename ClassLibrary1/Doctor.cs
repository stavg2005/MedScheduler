using System;
using System.Collections.Generic;

namespace Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        public int Workload { get; set; } // Current number of patients assigned
        public int MaxWorkload { get; set; } // Maximum number of patients the doctor can handle
        public List<string> Preferences { get; set; } = new List<string>(); // Preferences for types of patients
        public int ExperienceLevel { get; set; } // 1-Junior, 2-Regular, 3-Senior
        
        
        public List<int> PreviousPatients { get; set; } = new List<int>(); // IDs of patients previously treated by this doctor

        // Calculate current workload percentage
        public double WorkloadPercentage => MaxWorkload > 0 ? (double)Workload / MaxWorkload * 100 : 0;

        // Check if doctor can take a new patient
        public bool CanAcceptPatient() => Workload < MaxWorkload;

        // Check if doctor is suitable for a specific patient
        public bool IsSuitableFor(Patient patient)
        {
            return Specialization == patient.RequiredSpecialization &&
                   CanAcceptPatient() &&
                   (ExperienceLevel >= GetRequiredExperienceLevel(patient.Urgency));
        }

        private int GetRequiredExperienceLevel(string urgency)
        {
            switch (urgency.ToLower())
            {
                case "high": return 3; // High urgency requires senior doctors
                case "medium": return 2; // Medium urgency requires at least regular doctors
                default: return 1; // Low urgency can be handled by any doctor
            }
        }
    }
}