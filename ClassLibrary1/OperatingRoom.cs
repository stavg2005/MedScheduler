using System;
using System.Collections.Generic;

namespace Models
{
    public class OperatingRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> AvailableEquipment { get; set; } = new List<string>();
        public bool IsSpecialized { get; set; }
        public string Specialization { get; set; } // If this is a specialized operating room (e.g., cardiac surgery, neurosurgery)
        public Dictionary<DayOfWeek, List<TimeSpan>> AvailabilityHours { get; set; } = new Dictionary<DayOfWeek, List<TimeSpan>>();

        // Check if this operating room is suitable for a specific procedure
        public bool IsSuitableFor(MedicalProcedure procedure)
        {
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

        // Check if the room is available at a specific time
        public bool IsAvailableAt(DayOfWeek day, TimeSpan time, double durationHours)
        {
            if (!AvailabilityHours.ContainsKey(day))
            {
                return false;
            }

            foreach (var availableTime in AvailabilityHours[day])
            {
                // Check if the time slot can fit the procedure
                if (time >= availableTime && time.Add(TimeSpan.FromHours(durationHours)) <= availableTime.Add(TimeSpan.FromHours(2)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}