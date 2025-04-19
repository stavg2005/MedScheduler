using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class OperatingRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // public List<string> AvailableEquipment { get; set; } = new List<string>(); // REMOVED
        // public bool IsSpecialized { get; set; } // REMOVED
        // public string Specialization { get; set; } // REMOVED

        // Availability and scheduling remain
        public Dictionary<DayOfWeek, List<TimeRange>> AvailabilityHours { get; set; } = new Dictionary<DayOfWeek, List<TimeRange>>();
        public List<TimeSlot> ScheduledSlots { get; set; } = new List<TimeSlot>();

        // IsSuitableFor becomes trivial - always true as rooms are generic
        public bool IsSuitableFor(MedicalProcedure procedure)
        {
            return true; // All rooms are suitable for all procedures now
        }

        // Availability check remains the same
        public bool IsSlotGenerallyAvailableAndFree(DateTime requiredStart, DateTime requiredEnd)
        {
            DayOfWeek day = requiredStart.DayOfWeek;
            TimeSpan requiredStartTimeOfDay = requiredStart.TimeOfDay;
            TimeSpan requiredEndTimeOfDay = requiredEnd.TimeOfDay;

            bool withinGeneralAvailability = false;
            if (AvailabilityHours.TryGetValue(day, out var availableRanges))
            {
                foreach (var range in availableRanges)
                {
                    if (requiredStartTimeOfDay >= range.StartTime && requiredEndTimeOfDay <= range.EndTime)
                    {
                        withinGeneralAvailability = true;
                        break;
                    }
                }
            }
            if (!withinGeneralAvailability) return false;

            return !ScheduledSlots.Any(slot => slot.Overlaps(requiredStart, requiredEnd));
        }


    }
}