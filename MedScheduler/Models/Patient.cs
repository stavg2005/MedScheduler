using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedScheduler.Models
{
    internal class Patient
    {
        public int Id { get; set; }
        public string Condition { get; set; }
        public string Urgency { get; set; } // Urgency level: High, Medium, Low
        public string RequiredSpecialization { get; set; }
    }
}
