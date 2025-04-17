using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class Statistics
    {

        public int totalPaitents { get; set; }
        public int totalAssigned { get; set; }
        public double assignmentPercentage { get; set; }

        public double specializationMatchRate { get; set; }

        public double AvarageDoctorWorkLoad { get; set; }

        public double UrgentCasesPriority { get; set; }
    }
}
