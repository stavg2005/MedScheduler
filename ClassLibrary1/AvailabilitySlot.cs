using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class AvailabilitySlot
    {
        public DayOfWeek DayOfWeek { get; set; } // System.DayOfWeek enum
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? RoomId { get; set; }
    }
}
