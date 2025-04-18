using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class DoctorPreference
    {
        // Optional: Add PreferenceId if needed from DB
        // public int PreferenceId { get; set; }
        public int DoctorId { get; set; } // Link back to the Doctor
        public PreferenceType Type { get; set; }
        public PreferenceDirection Direction { get; set; }

        // Store the specific value the preference applies to
        public int? LevelValue { get; set; } // Stores int value of UrgencyLevel or ComplexityLevel Enum
        public string ConditionValue { get; set; } // Stores specific condition name/code
    }
}
