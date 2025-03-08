using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    class Surgeon :Doctor
    {
        public bool IsAvailableForSurgery { get; set; }

        public Dictionary<DayOfWeek, List<TimeSpan>> Availability { get; set; } = new Dictionary<DayOfWeek, List<TimeSpan>>();
    }
}
