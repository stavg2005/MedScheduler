using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string Specialization { get; set; }
        public int Workload { get; set; } // Current number of patients assigned
        public int MaxWorkload { get; set; } // Maximum number of patients the doctor can handle
        public List<string> Preferences { get; set; } // Preferences for types of patients
    }
}
