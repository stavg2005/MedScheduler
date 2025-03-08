using System;
using System.Collections.Generic;

namespace Models
{
    public class MedicalProcedure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RequiredSpecialization { get; set; }
        public double EstimatedDuration { get; set; } // In hours
        public bool IsOperation { get; set; } // Whether this procedure is a surgical operation
        public int ComplexityLevel { get; set; } // 1-Simple, 2-Moderate, 3-Complex
        public List<string> RequiredEquipment { get; set; } = new List<string>();
        public int MinimumDoctorExperienceLevel { get; set; } // Minimum experience level required to perform this procedure

        // Check if a doctor is qualified to perform this procedure
        public bool IsQualified(Doctor doctor)
        {
            return doctor.Specialization == RequiredSpecialization &&
                   doctor.ExperienceLevel >= MinimumDoctorExperienceLevel;
        }
    }
}