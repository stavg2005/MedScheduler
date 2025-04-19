using System;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary1;


namespace Models // Or your appropriate namespace
{
    /// <summary>
    /// Implements a greedy algorithm for scheduling surgeries over a defined period (e.g., a month).
    /// Uses simplified Operating Room logic (no specialization/equipment checks).
    /// </summary>
    public class SurgeryScheduler
    {
        private readonly List<Patient> patientsNeedingSurgery;
        private readonly List<Surgeon> availableSurgeons;
        private readonly List<OperatingRoom> operatingRooms; // Simplified OR model used
        private readonly Dictionary<int, MedicalProcedure> proceduresById; // Lookup for procedure details
        private readonly DateTime scheduleStartDate; // Start date of the scheduling period
        private readonly DateTime scheduleEndDate;   // End date (exclusive) of the scheduling period
        private readonly TimeSpan schedulingGranularity = TimeSpan.FromMinutes(30); // Time increments to check
        private readonly TimeSpan workDayStart = new TimeSpan(8, 0, 0); // Start of workday
        private readonly TimeSpan workDayEnd = new TimeSpan(17, 0, 0);   // End of workday

        /// <summary>
        /// Initializes the Surgery Scheduler for a specific period.
        /// </summary>
        /// <param name="allPatients">Full list of patients.</param>
        /// <param name="allDoctors">Full list of doctors (will filter for surgeons).</param>
        /// <param name="operatingRooms">List of available operating rooms (simplified model).</param>
        /// <param name="allProcedures">List of all available procedures.</param>
        /// <param name="periodStartDate">The first day of the scheduling period.</param>
        /// <param name="periodEndDate">The day AFTER the last day of the scheduling period.</param>
        public SurgeryScheduler(
            List<Patient> allPatients,
            List<Doctor> allDoctors,
            List<OperatingRoom> operatingRooms,
            List<MedicalProcedure> allProcedures,
            DateTime periodStartDate,
            DateTime periodEndDate)
        {
            // Validate inputs
            if (allPatients == null || allDoctors == null || operatingRooms == null || allProcedures == null)
                throw new ArgumentNullException("Input lists cannot be null.");
            if (periodEndDate <= periodStartDate)
                throw new ArgumentException("End date must be after start date.");

            // Prepare procedure lookup
            this.proceduresById = allProcedures.ToDictionary(proc => proc.Id, proc => proc);

            // Filter patients needing surgery, not yet scheduled, and having a valid procedure ID
            this.patientsNeedingSurgery = allPatients
                .Where(p => p.NeedsSurgery
                            && p.RequiredProcedureId.HasValue
                            && proceduresById.ContainsKey(p.RequiredProcedureId.Value) // Ensure procedure exists
                            && !p.ScheduledSurgeryDate.HasValue)
                .OrderByDescending(p => (int)p.Urgency) // Prioritize by urgency
                .ThenBy(p => p.AdmissionDate)          // Then by admission date
                .ToList();

            // Filter for available surgeons
            this.availableSurgeons = allDoctors
                .OfType<Surgeon>() // Get only Surgeon objects
                .Where(s => s.IsAvailableForSurgery) // Check the flag
                .ToList();

            this.operatingRooms = operatingRooms ?? new List<OperatingRoom>();
            this.scheduleStartDate = periodStartDate.Date;
            this.scheduleEndDate = periodEndDate.Date; // Use the date part

            // Clear potentially stale OR schedules for the target period before starting
            ClearOrSchedulesForPeriod(this.operatingRooms, this.scheduleStartDate, this.scheduleEndDate);
        }

        /// <summary>
        /// Attempts to schedule surgeries greedily for the defined period.
        /// Updates Patient objects directly upon successful scheduling.
        /// </summary>
        /// <returns>A list of patients who could not be scheduled within the period.</returns>
        public List<Patient> ScheduleSurgeries()
        {
            List<Patient> successfullyScheduled = new List<Patient>();
            // Start with the filtered list of patients needing surgery
            List<Patient> currentlyUnscheduled = new List<Patient>(patientsNeedingSurgery);
            HashSet<int> scheduledPatientIds = new HashSet<int>(); // Track IDs scheduled in this run

            Console.WriteLine($"Attempting to schedule surgeries from {scheduleStartDate:yyyy-MM-dd} to {scheduleEndDate.AddDays(-1):yyyy-MM-dd}");

            // Loop through each day in the defined scheduling period
            for (DateTime currentDay = scheduleStartDate; currentDay < scheduleEndDate; currentDay = currentDay.AddDays(1))
            {
                DayOfWeek currentDayOfWeek = currentDay.DayOfWeek;


                // Iterate through time slots within the working day using the defined granularity
                TimeSpan currentTime = workDayStart;
                while (currentTime < workDayEnd)
                {
                    DateTime potentialSlotStart = currentDay + currentTime;

                    // Try scheduling the highest priority remaining patient first for this slot
                    // Iterate over a copy as we might modify the list
                    foreach (var patient in currentlyUnscheduled.ToList())
                    {
                        // Skip if already scheduled in this run (shouldn't happen with list removal, but safe)
                        if (scheduledPatientIds.Contains(patient.Id)) continue;

                        // Get required procedure (already validated in constructor filter)
                        MedicalProcedure procedure = proceduresById[patient.RequiredProcedureId.Value];

                        // Calculate required end time based on procedure duration
                        TimeSpan estimatedDuration = TimeSpan.FromHours(procedure.EstimatedDuration);
                        DateTime potentialSlotEnd = potentialSlotStart + estimatedDuration;

                        // Check if the calculated end time exceeds the workday for the current day
                        if (potentialSlotEnd.TimeOfDay > workDayEnd || potentialSlotEnd.Date > potentialSlotStart.Date)
                        {
                            continue; // Procedure won't fit starting at this time today
                        }

                        // --- Find Suitable and Available Surgeon ---
                        Surgeon surgeon = FindAvailableSurgeonForSlot(procedure, potentialSlotStart, potentialSlotEnd);

                        if (surgeon != null)
                        {
                            // --- Find Available Operating Room ---
                            // No suitability check needed as ORs are now generic
                            OperatingRoom or = FindAvailableOperatingRoomForSlot(potentialSlotStart, potentialSlotEnd);

                            if (or != null)
                            {
                                // --- Assignment Found! ---
                                Console.WriteLine($"  Scheduled: P{patient.Id} ({procedure.Name}) with S{surgeon.Id} in OR{or.Id} at {potentialSlotStart:yyyy-MM-dd HH:mm}");

                                // Update Patient object directly
                                patient.ScheduledSurgeryDate = potentialSlotStart;
                                patient.AssignedSurgeonId = surgeon.Id;
                                patient.AssignedOperatingRoomId = or.Id;

                                // Add busy slot to the OR's schedule for this run
                                or.ScheduledSlots.Add(new TimeSlot
                                {
                                    StartTime = potentialSlotStart,
                                    EndTime = potentialSlotEnd,
                                    PatientId = patient.Id,
                                    SurgeonId = surgeon.Id,
                                    OperatingRoomId = or.Id
                                });

                                // Mark patient as scheduled for this run
                                successfullyScheduled.Add(patient);
                                currentlyUnscheduled.Remove(patient); // Remove from the list to schedule
                                scheduledPatientIds.Add(patient.Id);

                                // Since we filled this slot for this OR/Surgeon (implicitly),
                                // move to the next time slot. A more complex version might
                                // check if other ORs/Surgeons are free in the *same* slot.
                                goto AdvanceTimeSlot;
                            }
                            // else: No OR found for this surgeon/time
                        }
                        // else: No surgeon found for this patient/time
                    } // End loop through patients for this slot

                AdvanceTimeSlot: // Label to jump to after processing a time slot
                    currentTime = currentTime.Add(schedulingGranularity); // Advance time

                } // End time slot loop for day
            } // End day loop

            Console.WriteLine($"Surgery scheduling complete for period. Successful: {successfullyScheduled.Count}, Unscheduled: {currentlyUnscheduled.Count}");
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
                // Find surgeons qualified for the procedure
                .Where(s => procedure.IsQualified(s))
                // Find one who has a matching availability pattern and isn't otherwise booked
                .FirstOrDefault(s =>
                    s.Availability != null &&
                    s.Availability.Any(availSlot =>
                        availSlot.DayOfWeek == dayOfWeek &&
                        availSlot.StartTime <= startTimeOfDay && // Slot must start within or at start of available block
                        availSlot.EndTime >= endTimeOfDay) &&    // Slot must end within or at end of available block
                    IsSurgeonFree(s.Id, slotStart, slotEnd) // Check against actual bookings (placeholder)
                );
        }

        /// <summary>
        /// Finds an OR available during the specified time slot (suitability check removed).
        /// </summary>
        private OperatingRoom FindAvailableOperatingRoomForSlot(DateTime slotStart, DateTime slotEnd)
        {
            // Only check time availability now, suitability is always true for generic ORs
            return operatingRooms.FirstOrDefault(or => or.IsSlotGenerallyAvailableAndFree(slotStart, slotEnd));
        }

        /// <summary>
        /// Placeholder: Checks if a surgeon is already booked for another procedure during this time.
        /// Needs implementation if tracking surgeon bookings is required.
        /// </summary>
        private bool IsSurgeonFree(int surgeonId, DateTime slotStart, DateTime slotEnd)
        {
            // TODO: Implement check against a surgeon schedule tracking mechanism.
            // This could involve looking at the 'successfullyScheduled' list during the run,
            // or querying a persistent booking table if this were integrated with a larger system.
            // Returning true for now assumes the OR availability is the main constraint.
            return true;
        }

        /// <summary>
        /// Helper to clear scheduled slots for the target period from ORs.
        /// </summary>
        private void ClearOrSchedulesForPeriod(List<OperatingRoom> ors, DateTime periodStart, DateTime periodEnd)
        {
            if (ors == null) return;
            foreach (var or in ors)
            {
                // Remove slots that START within the specified period
                or.ScheduledSlots?.RemoveAll(slot => slot.StartTime >= periodStart && slot.StartTime < periodEnd);
            }
            Console.WriteLine($"Cleared existing OR schedules between {periodStart:yyyy-MM-dd} and {periodEnd.AddDays(-1):yyyy-MM-dd}");
        }
    }
}
