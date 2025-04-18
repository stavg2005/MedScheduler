using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? PatientId { get; set; }
        public int? SurgeonId { get; set; }
        public int? OperatingRoomId { get; set; } // Added for context

        public bool Overlaps(DateTime otherStart, DateTime otherEnd)
        {
            return this.StartTime < otherEnd && this.EndTime > otherStart;
        }
    }
}
