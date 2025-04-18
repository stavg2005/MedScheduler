using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class Surgeon :Doctor
    {
        public bool IsAvailableForSurgery { get; set; }

        public List<AvailabilitySlot> Availability { get; set; } = new List<AvailabilitySlot>();
    }
}
