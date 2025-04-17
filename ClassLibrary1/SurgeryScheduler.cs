using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace Models
{
    public class SurgeryScheduler
    {
        #region Data Members
        // Specialized collections for surgery scheduling
        private readonly ConcurrentDictionary<int, Patient> PatientsById;
        private readonly ConcurrentDictionary<int, Doctor> SurgeonsById;
        private readonly ConcurrentDictionary<int, OperatingRoom> OperatingRoomsById;

        // AVL Tree replacements using SortedDictionary
        private readonly SortedDictionary<string, List<Doctor>> SurgeonsBySpecialization;
        private readonly SortedDictionary<int, List<OperatingRoom>> RoomsBySpecializationMatch;

        // Priority queue using sorted list for patients by urgency
        private readonly SortedList<(int UrgencyValue, int PatientId), Patient> PatientQueue;

        // Schedule tracking
        private readonly Schedule surgerySchedule;
        private readonly DateTime startDate;
        private readonly int scheduleDays;

        // Statistics tracking
        private int totalSurgeryPatients = 0;
        public int scheduledSurgeries = 0;
        private int unscheduledSurgeries = 0;
        #endregion

        #region Constructor
        public SurgeryScheduler(List<Patient> patients, List<Doctor> surgeons, List<OperatingRoom> operatingRooms,
                              DateTime startDate, int daysToSchedule = 7)
        {
            // Initialize collections
            PatientsById = new ConcurrentDictionary<int, Patient>();
            SurgeonsById = new ConcurrentDictionary<int, Doctor>();
            OperatingRoomsById = new ConcurrentDictionary<int, OperatingRoom>();

            SurgeonsBySpecialization = new SortedDictionary<string, List<Doctor>>();
            RoomsBySpecializationMatch = new SortedDictionary<int, List<OperatingRoom>>();

            // Custom comparer to prioritize by urgency first, then by patient ID
            PatientQueue = new SortedList<(int UrgencyValue, int PatientId), Patient>(
                Comparer<(int UrgencyValue, int PatientId)>.Create((a, b) => {
                    // Sort by urgency descending
                    int urgencyComparison = b.UrgencyValue.CompareTo(a.UrgencyValue);
                    if (urgencyComparison != 0) return urgencyComparison;

                    // If urgency is the same, sort by patient ID ascending (deterministic)
                    return a.PatientId.CompareTo(b.PatientId);
                })
            );

            // Initialize schedule and dates
            surgerySchedule = new Schedule();
            this.startDate = startDate;
            this.scheduleDays = daysToSchedule;

            // Populate data structures
            PopulateDataStructures(patients, surgeons, operatingRooms);
        }
        #endregion

        #region Data Structure Setup
        private void PopulateDataStructures(List<Patient> patients, List<Doctor> surgeons, List<OperatingRoom> operatingRooms)
        {
            // Process patients
            foreach (var patient in patients)
            {
                if (patient.NeedsSurgery)
                {
                    totalSurgeryPatients++;
                    PatientsById[patient.Id] = patient;

                    // Add to priority queue
                    PatientQueue.Add((patient.GetUrgencyValue(), patient.Id), patient);
                }
            }

            // Process surgeons
            foreach (var surgeon in surgeons)
            {
                SurgeonsById[surgeon.Id] = surgeon;

                // Group by specialization
                if (!SurgeonsBySpecialization.ContainsKey(surgeon.Specialization))
                {
                    SurgeonsBySpecialization[surgeon.Specialization] = new List<Doctor>();
                }
                SurgeonsBySpecialization[surgeon.Specialization].Add(surgeon);
            }

            // Process operating rooms
            foreach (var room in operatingRooms)
            {
                OperatingRoomsById[room.Id] = room;

                // Group by specialization match score 
                // (higher score means the room is more specialized with more equipment)
                int specializationScore = room.IsSpecialized ? 2 : 1;
                if (!RoomsBySpecializationMatch.ContainsKey(specializationScore))
                {
                    RoomsBySpecializationMatch[specializationScore] = new List<OperatingRoom>();
                }
                RoomsBySpecializationMatch[specializationScore].Add(room);
            }
        }
        #endregion

        #region Main Scheduling Algorithm
        public Schedule ScheduleSurgeries()
        {
            Console.WriteLine($"Starting surgery scheduling for {totalSurgeryPatients} patients...");
            Console.WriteLine($"Available surgeons: {SurgeonsById.Count}");
            Console.WriteLine($"Available operating rooms: {OperatingRoomsById.Count}");

            // Pre-allocate all time slots for the schedule
            InitializeScheduleTimeSlots();

            // Process patients in priority order
            while (PatientQueue.Count > 0)
            {
                // Get highest priority patient
                var firstKey = PatientQueue.Keys[0];
                var patient = PatientQueue[firstKey];
                PatientQueue.RemoveAt(0);

                Console.WriteLine($"Processing patient {patient.Id}: {patient.Condition} (Urgency: {patient.Urgency})");

                // Try to schedule this patient
                if (TryScheduleSurgery(patient))
                {
                    
                    scheduledSurgeries++;
                    Console.WriteLine($"✓ Scheduled surgery for patient {patient.Id}");
                }
                else
                {
                    unscheduledSurgeries++;
                    Console.WriteLine($"✗ Could not schedule surgery for patient {patient.Id}");
                }
            }

            // Print schedule statistics
            PrintScheduleStatistics();

            return surgerySchedule;
        }

        private void InitializeScheduleTimeSlots()
        {
            // Initialize the schedule with empty slots
            for (int day = 0; day < scheduleDays; day++)
            {
                DateTime currentDate = startDate.AddDays(day);
                DayOfWeek dayOfWeek = currentDate.DayOfWeek;

                // Skip weekends if applicable
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                // Initialize this day in the surgery schedule
                if (!surgerySchedule.SurgerySchedule.ContainsKey(currentDate))
                {
                    surgerySchedule.SurgerySchedule[currentDate] = new Dictionary<int, List<int>>();
                }

                // Initialize all operating rooms for this day
                foreach (var roomId in OperatingRoomsById.Keys)
                {
                    surgerySchedule.SurgerySchedule[currentDate][roomId] = new List<int>();
                }
            }
        }

        private bool TryScheduleSurgery(Patient patient)
        {
            // Step 1: Find matching surgeon
            Doctor matchingSurgeon = FindSuitableSurgeon(patient);
            if (matchingSurgeon == null)
            {
                Console.WriteLine($"  No suitable surgeon found for patient {patient.Id}");
                return false;
            }

            // IMPORTANT: Assign surgeon to patient
            patient.AssignedSurgeonId = matchingSurgeon.Id;

            // Step 2: Find matching operating room
            var result = FindSuitableRoomAndDate(patient, matchingSurgeon);
            if (!result.success)
            {
                Console.WriteLine($"  No suitable operating room or date found for patient {patient.Id}");
                return false;
            }

            // Step 3: Schedule the surgery
            OperatingRoom selectedRoom = result.room;
            DateTime selectedDate = result.date;

            // Add to schedule
            surgerySchedule.SurgerySchedule[selectedDate][selectedRoom.Id].Add(patient.Id);

            // Update patient record
            patient.ScheduledSurgeryDate = selectedDate;

            // Make sure to update the patient-to-doctor mapping in the schedule
            if (!surgerySchedule.PatientToDoctor.ContainsKey(patient.Id))
            {
                surgerySchedule.PatientToDoctor[patient.Id] = matchingSurgeon.Id;
            }

            // Update the doctor-to-patients mapping
            if (!surgerySchedule.DoctorToPatients.ContainsKey(matchingSurgeon.Id))
            {
                surgerySchedule.DoctorToPatients[matchingSurgeon.Id] = new List<int>();
            }

            if (!surgerySchedule.DoctorToPatients[matchingSurgeon.Id].Contains(patient.Id))
            {
                surgerySchedule.DoctorToPatients[matchingSurgeon.Id].Add(patient.Id);
            }

            Console.WriteLine($"  Scheduled for {selectedDate.ToShortDateString()} with Dr. {matchingSurgeon.Name} in Room {selectedRoom.Id}");

            return true;
        }
        #endregion

        #region Finding Resources
        private Doctor FindSuitableSurgeon(Patient patient)
        {
            // First try to find surgeons with the right specialization
            if (SurgeonsBySpecialization.TryGetValue(patient.RequiredSpecialization, out var specialists))
            {
                // Find available specialists sorted by most experienced first for high urgency,
                // and by least loaded first for lower urgency
                var availableSpecialists = specialists
                    .Where(s => IsAvailableForNewSurgery(s))
                    .OrderBy(s => patient.GetUrgencyValue() == 3 ? -s.ExperienceLevel : s.Workload)
                    .ToList();

                if (availableSpecialists.Any())
                {
                    return availableSpecialists.First();
                }
            }

            // If no specialists are available, for high urgency cases we can try other surgeons
            if (patient.GetUrgencyValue() == 3)
            {
                var anySurgeon = SurgeonsById.Values
                    .Where(s => IsAvailableForNewSurgery(s))
                    .OrderByDescending(s => s.ExperienceLevel)
                    .FirstOrDefault();

                return anySurgeon;
            }

            return null;
        }

        private (bool success, OperatingRoom room, DateTime date) FindSuitableRoomAndDate(Patient patient, Doctor surgeon)
        {
            // Go through each possible date in our scheduling window
            for (int day = 0; day < scheduleDays; day++)
            {
                DateTime currentDate = startDate.AddDays(day);
                DayOfWeek dayOfWeek = currentDate.DayOfWeek;

                // Skip weekends
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                // First look for specialized rooms if applicable
                foreach (var specializationScore in RoomsBySpecializationMatch.Keys.OrderByDescending(k => k))
                {
                    foreach (var room in RoomsBySpecializationMatch[specializationScore])
                    {
                        // Check if the room is suitable for this surgery and available on this date
                        if (IsRoomSuitableForSurgery(room, patient) &&
                            IsRoomAvailableOnDate(room, currentDate))
                        {
                            return (true, room, currentDate);
                        }
                    }
                }
            }

            // If high urgency and still not found a slot, check if we can find ANY room
            if (patient.GetUrgencyValue() == 3)
            {
                for (int day = 0; day < scheduleDays; day++)
                {
                    DateTime currentDate = startDate.AddDays(day);
                    if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    foreach (var room in OperatingRoomsById.Values)
                    {
                        if (IsRoomAvailableOnDate(room, currentDate))
                        {
                            return (true, room, currentDate);
                        }
                    }
                }
            }

            return (false, null, DateTime.MinValue);
        }
        #endregion

        #region Helper Methods
        private bool IsAvailableForNewSurgery(Doctor surgeon)
        {
            // Implement logic for surgeon availability
            // This is a simplified version - you might have more complex rules
            return surgeon.Workload < surgeon.MaxWorkload;
        }

        private bool IsRoomSuitableForSurgery(OperatingRoom room, Patient patient)
        {
            // Room is specialized for this kind of surgery
            if (room.IsSpecialized)
            {
                return room.Specialization == patient.RequiredSpecialization;
            }

            // General operating room - can handle any surgery
            return true;
        }

        private bool IsRoomAvailableOnDate(OperatingRoom room, DateTime date)
        {
            // Check if room is already scheduled for this date
            if (!surgerySchedule.SurgerySchedule.ContainsKey(date) ||
                !surgerySchedule.SurgerySchedule[date].ContainsKey(room.Id))
            {
                return true;
            }

            // Check if the room already has a surgery scheduled
            return surgerySchedule.SurgerySchedule[date][room.Id].Count == 0;
        }

        private void PrintScheduleStatistics()
        {
            Console.WriteLine();
            Console.WriteLine("=== Surgery Schedule Statistics ===");
            Console.WriteLine($"Total patients needing surgery: {totalSurgeryPatients}");
            Console.WriteLine($"Scheduled surgeries: {scheduledSurgeries}");
            Console.WriteLine($"Unscheduled surgeries: {unscheduledSurgeries}");

            double scheduledPercentage = totalSurgeryPatients > 0 ?
                (double)scheduledSurgeries / totalSurgeryPatients * 100 : 0;

            Console.WriteLine($"Scheduling success rate: {scheduledPercentage:F1}%");

            // Calculate utilization
            int totalSlots = 0;
            int usedSlots = 0;

            foreach (var dateEntry in surgerySchedule.SurgerySchedule)
            {
                foreach (var roomEntry in dateEntry.Value)
                {
                    totalSlots++;
                    if (roomEntry.Value.Count > 0)
                    {
                        usedSlots++;
                    }
                }
            }

            double utilizationRate = totalSlots > 0 ? (double)usedSlots / totalSlots * 100 : 0;
            Console.WriteLine($"Operating room utilization: {utilizationRate:F1}%");

            // Print schedule by day
            Console.WriteLine();
            Console.WriteLine("Schedule by day:");

            foreach (var dateEntry in surgerySchedule.SurgerySchedule.OrderBy(e => e.Key))
            {
                int surgeriesThisDay = dateEntry.Value.Sum(r => r.Value.Count);
                if (surgeriesThisDay > 0)
                {
                    Console.WriteLine($"{dateEntry.Key.ToShortDateString()}: {surgeriesThisDay} surgeries");

                    // Print details of each surgery
                    foreach (var roomEntry in dateEntry.Value.Where(r => r.Value.Count > 0))
                    {
                        OperatingRoom room = OperatingRoomsById[roomEntry.Key];
                        foreach (int patientId in roomEntry.Value)
                        {
                            Patient patient = PatientsById[patientId];
                            Doctor surgeon = SurgeonsById[patient.AssignedSurgeonId.Value];

                            Console.WriteLine($"  Room {room.Id}: Patient {patientId} ({patient.Condition}) - Dr. {surgeon.Name}");
                        }
                    }
                }
            }
        }
        #endregion
    }
}