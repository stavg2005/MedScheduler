using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Schedule
    {
        public Dictionary<int, List<int>> DoctorToPatients { get; set; } // Doctor ID to list of patient IDs
        public Dictionary<int, int> PatientToDoctor { get; set; } // Patient ID to Doctor ID
    }
}
