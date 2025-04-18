using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class OperatingRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> AvailableEquipment { get; set; } = new List<string>();
        public bool IsSpecialized { get; set; }
        public string Specialization { get; set; } // If this is a specialized operating room (e.g., cardiac surgery, neurosurgery)
        public Dictionary<DayOfWeek, List<TimeRange>> AvailabilityHours { get; set; } = new Dictionary<DayOfWeek, List<TimeRange>>();

        public List<TimeSlot> ScheduledSlots { get; set; } = new List<TimeSlot>();

        // Check if this operating room is suitable for a specific procedure
        public bool IsSuitableFor(MedicalProcedure procedure)
        {
            if (procedure == null) return false;
            // If the room is specialized, check if it matches the procedure's specialization
            if (IsSpecialized && Specialization != procedure.RequiredSpecialization)
            {
                return false;
            }

            // Check if the room has all required equipment
            foreach (var equipment in procedure.RequiredEquipment)
            {
                if (!AvailableEquipment.Contains(equipment))
                {
                    return false;
                }
            }

            return true;
        }

        // <summary>
        /// Finds a specific time slot within the OR's general availability and checks if it's free from scheduled procedures.
        /// </summary>
        /// <param name="requiredStart">The desired start time.</param>
        /// <param name="requiredEnd">The desired end time.</param>
        /// <returns>True if the slot is potentially available and not already booked, false otherwise.</returns>
        public bool IsSlotGenerallyAvailableAndFree(DateTime requiredStart, DateTime requiredEnd)
        {
            DayOfWeek day = requiredStart.DayOfWeek;
            TimeSpan startTimeOfDay = requiredStart.TimeOfDay;
            TimeSpan endTimeOfDay = requiredEnd.TimeOfDay;

            // 1. Check if the slot falls within the general AvailabilityHours for that day
            bool withinGeneralAvailability = false;
            if (AvailabilityHours.TryGetValue(day, out var availableRanges))
            {
                foreach (var range in availableRanges)
                {
                    // Check if the required slot is fully contained within an available range
                    if (startTimeOfDay >= range.StartTime && endTimeOfDay <= range.EndTime)
                    {
                        withinGeneralAvailability = true;
                        break;
                    }
                }
            }

            if (!withinGeneralAvailability)
            {
                return false; // Not even generally available
            }

            // 2. Check if the specific slot overlaps with already scheduled slots
            return !ScheduledSlots.Any(slot => slot.Overlaps(requiredStart, requiredEnd));
        }
    
        
    }
}