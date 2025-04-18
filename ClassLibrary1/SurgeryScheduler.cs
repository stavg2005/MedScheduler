using System;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary1;




namespace Models 
{
    public class SurgeryScheduler
    {
        private readonly List<Patient> patientsToSchedule;
        private readonly List<Surgeon> availableSurgeons;
        private readonly List<OperatingRoom> operatingRooms;
        private readonly Dictionary<int, MedicalProcedure> proceduresById; // Lookup for procedure details
        private readonly DateTime schedulingWeekStart;
        private readonly TimeSpan schedulingGranularity = TimeSpan.FromMinutes(30); // Schedule in 30-min increments
        private readonly TimeSpan workDayStart = new TimeSpan(8, 0, 0);
        private readonly TimeSpan workDayEnd = new TimeSpan(17, 0, 0);

        public SurgeryScheduler(
            List<Patient> allPatients,
            List<Doctor> allDoctors, // Will filter surgeons internally
            List<OperatingRoom> operatingRooms,
            List<MedicalProcedure> allProcedures,
            DateTime schedulingWeekStartDate)
        {
            if (allPatients == null || allDoctors == null || operatingRooms == null || allProcedures == null)
                throw new ArgumentNullException("Input lists cannot be null.");

            // Filter patients needing surgery and not yet scheduled
            this.patientsToSchedule = allPatients
                .Where(p => p.NeedsSurgery && p.RequiredProcedureId.HasValue && !p.ScheduledSurgeryDate.HasValue)
                .OrderByDescending(p => (int)p.Urgency)
                .ThenBy(p => p.AdmissionDate)
                .ToList();

            // Filter for available surgeons
            this.availableSurgeons = allDoctors
                .OfType<Surgeon>()
                .Where(s => s.IsAvailableForSurgery)
                .ToList();

            this.operatingRooms = operatingRooms;

            // Create a lookup dictionary for procedures
            this.proceduresById = allProcedures.ToDictionary(proc => proc.Id, proc => proc);

            if (schedulingWeekStartDate.DayOfWeek != DayOfWeek.Monday)
                throw new ArgumentException("Scheduling week start date must be a Monday.", nameof(schedulingWeekStartDate));
            this.schedulingWeekStart = schedulingWeekStartDate.Date;

            // Clear potentially stale schedules from ORs for the target week
            ClearOrSchedulesForWeek(this.operatingRooms, this.schedulingWeekStart);
        }

        /// <summary>
        /// Attempts to schedule surgeries greedily for the upcoming week.
        /// Updates Patient objects directly upon successful scheduling.
        /// </summary>
        /// <returns>A list of patients who could not be scheduled.</returns>
        public List<Patient> ScheduleSurgeries()
        {
            List<Patient> successfullyScheduled = new List<Patient>();
            List<Patient> currentlyUnscheduled = new List<Patient>(patientsToSchedule); // Start with all needing scheduling

            // Iterate through days (Mon-Fri)
            for (int dayOffset = 0; dayOffset < 5; dayOffset++)
            {
                DateTime currentDay = schedulingWeekStart.AddDays(dayOffset);
                DayOfWeek currentDayOfWeek = currentDay.DayOfWeek;

                // Iterate through time slots using granularity
                TimeSpan currentTime = workDayStart;
                while (currentTime < workDayEnd)
                {
                    DateTime potentialSlotStart = currentDay + currentTime;

                    // Try scheduling patients (highest urgency first) for this potential start time
                    // Iterate over a copy because we modify the list
                    foreach (var patient in currentlyUnscheduled.ToList())
                    {
                        // Get required procedure details
                        if (!patient.RequiredProcedureId.HasValue ||
                            !proceduresById.TryGetValue(patient.RequiredProcedureId.Value, out var procedure))
                        {
                            continue; // Skip patient if procedure details are missing
                        }

                        TimeSpan estimatedDuration = TimeSpan.FromHours(procedure.EstimatedDuration);
                        DateTime potentialSlotEnd = potentialSlotStart + estimatedDuration;

                        // Check if slot exceeds workday
                        if (potentialSlotEnd.TimeOfDay > workDayEnd || potentialSlotEnd.Date > potentialSlotStart.Date)
                        {
                            continue; // Doesn't fit within the workday starting at this time
                        }

                        // --- Find suitable and available Surgeon ---
                        Surgeon surgeon = FindAvailableSurgeonForSlot(procedure, potentialSlotStart, potentialSlotEnd);

                        if (surgeon != null)
                        {
                            // --- Find suitable and available Operating Room ---
                            OperatingRoom or = FindAvailableOperatingRoomForSlot(procedure, potentialSlotStart, potentialSlotEnd);

                            if (or != null)
                            {
                                // --- Assignment Found! ---
                                Console.WriteLine($"Attempting schedule: P{patient.Id} with S{surgeon.Id} in OR{or.Id} at {potentialSlotStart}");

                                // Update Patient record
                                patient.ScheduledSurgeryDate = potentialSlotStart;
                                patient.AssignedSurgeonId = surgeon.Id;
                                patient.AssignedOperatingRoomId = or.Id; // Assign OR ID

                                // Add busy slot to OR's schedule
                                or.ScheduledSlots.Add(new TimeSlot
                                {
                                    StartTime = potentialSlotStart,
                                    EndTime = potentialSlotEnd,
                                    PatientId = patient.Id,
                                    SurgeonId = surgeon.Id,
                                    OperatingRoomId = or.Id
                                });

                                // Mark patient as scheduled and remove from pool for this run
                                successfullyScheduled.Add(patient);
                                currentlyUnscheduled.Remove(patient);

                                // --- IMPORTANT: Potentially block surgeon/OR for subsequent checks in *this* timeslot iteration ---
                                // This simple greedy approach moves to the next time slot after finding one match.
                                // A more complex approach might try to fill the *same* timeslot with other ORs/Surgeons
                                // before advancing time. For now, we advance time.

                                // Since we scheduled someone, break the inner patient loop and advance time
                                goto AdvanceTimeSlot; // Use goto for simplicity to break outer loop and advance time
                            }
                        }
                    } // End patient loop for this slot

                AdvanceTimeSlot: // Label to jump to after successful scheduling or trying all patients
                    currentTime = currentTime.Add(schedulingGranularity); // Advance to next potential start time

                } // End time slot loop for day
            } // End day loop

            Console.WriteLine($"Scheduling complete. Successful: {successfullyScheduled.Count}, Unscheduled: {currentlyUnscheduled.Count}");
            return currentlyUnscheduled; // Return the list of patients who remain unscheduled
        }

        /// <summary>
        /// Finds a surgeon qualified for the procedure and available during the specified time slot.
        /// </summary>
        private Surgeon FindAvailableSurgeonForSlot(MedicalProcedure procedure, DateTime slotStart, DateTime slotEnd)
        {
            DayOfWeek dayOfWeek = slotStart.DayOfWeek;
            TimeSpan startTimeOfDay = slotStart.TimeOfDay;
            TimeSpan endTimeOfDay = slotEnd.TimeOfDay;

            return availableSurgeons
                .Where(s => procedure.IsQualified(s)) // Check specialization and experience
                .FirstOrDefault(s => // Check availability
                    s.Availability != null &&
                    s.Availability.Any(availSlot =>
                        availSlot.DayOfWeek == dayOfWeek &&
                        availSlot.StartTime <= startTimeOfDay && // Required start is within or at start of available slot
                        availSlot.EndTime >= endTimeOfDay) &&    // Required end is within or at end of available slot
                                                                 // --- Add check against actual surgeon bookings if tracked separately ---
                    IsSurgeonFree(s.Id, slotStart, slotEnd) // Placeholder for actual booking check
                );
        }

        /// <summary>
        /// Finds an OR suitable for the procedure and available during the specified time slot.
        /// </summary>
        private OperatingRoom FindAvailableOperatingRoomForSlot(MedicalProcedure procedure, DateTime slotStart, DateTime slotEnd)
        {
            return operatingRooms
                .Where(or => or.IsSuitableFor(procedure)) // Check specialization and equipment
                .FirstOrDefault(or => or.IsSlotGenerallyAvailableAndFree(slotStart, slotEnd)); // Check general hours AND booked slots
        }

        /// <summary>
        /// Placeholder: Checks if a surgeon is already booked for another procedure during this time.
        /// Requires tracking surgeon schedules similar to OR schedules.
        /// </summary>
        private bool IsSurgeonFree(int surgeonId, DateTime slotStart, DateTime slotEnd)
        {
            // TODO: Implement actual check against a surgeon schedule tracking mechanism
            // For now, assume availability list is sufficient (simplification)
            return true;
        }

        /// <summary>
        /// Clears scheduled slots for the target week from ORs.
        /// </summary>
        private void ClearOrSchedulesForWeek(List<OperatingRoom> ors, DateTime weekStart)
        {
            DateTime weekEnd = weekStart.AddDays(7);
            foreach (var or in ors)
            {
                // Clear only slots relevant to this scheduler run if needed,
                // or clear all within the week.
                or.ScheduledSlots.RemoveAll(slot => slot.StartTime >= weekStart && slot.StartTime < weekEnd);
            }
        }
    }
}
