using ClassLibrary1;
using System;
using System.Collections.Generic;

namespace Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; } // Consider a dedicated class or lookup table
        public int Workload { get; set; } // Current number of patients assigned
        public int MaxWorkload { get; set; } // Maximum number of patients the doctor can handle
        public ExperienceLevel ExperienceLevel { get; set; } // Using Enum

        public List<int> patientsIDS { get; set; }
        public List<DoctorPreference> Preferences { get; set; } = new List<DoctorPreference>();
        public List<int> PreviousPatients { get; set; } = new List<int>(); // IDs of patients previously treated

        // Calculated property for workload percentage
        public double WorkloadPercentage => MaxWorkload > 0 ? (double)Workload / MaxWorkload * 100 : 0;

        // Check if doctor can take a new patient based on workload
        public virtual bool CanAcceptPatient() => Workload < MaxWorkload;

        // Check if doctor is suitable for a specific patient based on specialization, workload, and experience vs urgency
        public virtual bool IsSuitableFor(Patient patient)
        {
            // Basic suitability check (can be overridden by Surgeon if needed)
            return Specialization == patient.RequiredSpecialization &&
                   CanAcceptPatient() &&
                   (ExperienceLevel >= GetRequiredExperienceLevel(patient.Urgency));
        }
        public void SetCurrentWorkLoad()
        {
            Workload = patientsIDS.Count;
        }

        // Helper to determine required experience based on patient urgency
        private ExperienceLevel GetRequiredExperienceLevel(UrgencyLevel urgency)
        {
            switch (urgency)
            {
                case UrgencyLevel.High: return ExperienceLevel.Senior;
                case UrgencyLevel.Medium: return ExperienceLevel.Regular;
                default: return ExperienceLevel.Junior; // Low urgency
            }
        }
    }
}