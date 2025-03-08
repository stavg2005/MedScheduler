using System;
using System.Collections.Generic;

namespace Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Condition { get; set; }
        public string Urgency { get; set; } // Urgency level: High, Medium, Low
        public string RequiredSpecialization { get; set; }
        public bool NeedsSurgery { get; set; }
        public DateTime AdmissionDate { get; set; }
        public DateTime? ScheduledSurgeryDate { get; set; }
        public int? AssignedDoctorId { get; set; }
        public int? AssignedSurgeonId { get; set; }
        public List<int> PreviousDoctors { get; set; } = new List<int>(); // History of doctors who treated this patient
        public int ComplexityLevel { get; set; } // 1-Simple, 2-Moderate, 3-Complex
        public double EstimatedTreatmentTime { get; set; } // In hours

        // Utility method to get urgency as an integer value
        public int GetUrgencyValue()
        {
            switch (Urgency.ToLower())
            {
                case "high": return 3;
                case "medium": return 2;
                case "low": return 1;
                default: return 0;
            }
        }

        // Check if there is continuity of care with a specific doctor
        public bool HasContinuityOfCare(int doctorId)
        {
            return PreviousDoctors.Contains(doctorId);
        }
    }
}