using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using ClassLibrary1;
using DB;
namespace Models
{
    public class SchedulerOrchestrator
    {
        public readonly List<Doctor> doctors;
        public readonly List<Patient> patients;
        public readonly List<OperatingRoom> operatingRooms;

        // Parameters for genetic algorithm
        public readonly int populationSize = 300;
        public readonly int maxGenerations = 500;

        // For tracking performance
        public readonly Stopwatch stopwatch = new Stopwatch();

        // Main schedule
        public Schedule mainSchedule;
        public Statistics stat = new Statistics();
        public Schedule surgerySchedule;
        public int totalAssigned;
        public int totalSurgeries = 0;

        public SchedulerOrchestrator()
        {
            
            this.doctors = DataSingelton.Instance.Doctors;
            this.patients = DataSingelton.Instance.Patients;
            this.operatingRooms = DataSingelton.Instance.OperatingRooms;
        }

        public async Task<Schedule> GenerateOptimalSchedule()
        {
            stopwatch.Start();

            Console.WriteLine("=== Starting MedScheduler Optimization ===");
            Console.WriteLine($"Total doctors: {doctors.Count}");
            Console.WriteLine($"Total patients: {patients.Count}");
            Console.WriteLine($"Patients requiring surgery: {patients.Count(p => p.NeedsSurgery)}");

            // Step 1: Split patients by need (regular vs surgery)
            var regularPatients = patients.Where(p => !p.NeedsSurgery).ToList();
            var surgeryPatients = patients.Where(p => p.NeedsSurgery).ToList();

            Console.WriteLine($"\nRegular patients for doctor assignment: {regularPatients.Count}");
            Console.WriteLine($"Surgery patients for OR scheduling: {surgeryPatients.Count}");

            // Step A: First do the surgery scheduling with the greedy algorithm
            if (surgeryPatients.Any() && operatingRooms.Any())
            {
                Console.WriteLine("\n=== Starting Surgery Scheduling (Greedy Algorithm) ===");

                var surgeryScheduler = new SurgeryScheduler(
                    surgeryPatients,
                    doctors,
                    operatingRooms,
                    DateTime.Now.Date
                );

                surgerySchedule = surgeryScheduler.ScheduleSurgeries();

                foreach (var patient in surgeryPatients)
                {
                    var originalPatient = patients.FirstOrDefault(p => p.Id == patient.Id);
                    if (originalPatient != null && patient.AssignedSurgeonId.HasValue)
                    {
                        // Copy the surgeon assignment back to the original patient
                        originalPatient.AssignedSurgeonId = patient.AssignedSurgeonId;
                    }
                }


                Console.WriteLine($"Surgery scheduling completed in {stopwatch.ElapsedMilliseconds / 1000.0:F1} seconds");
            }

            // Step B: Assign regular patients to doctors with genetic algorithm
            Schedule doctorSchedule = null;
            if (regularPatients.Any())
            {
                Console.WriteLine("\n=== Starting Doctor Assignment (Genetic Algorithm) ===");

                var geneticScheduler = new DoctorScheduler(
                    populationSize,
                    doctors,
                    regularPatients
                );

                doctorSchedule =  geneticScheduler.Solve();

                Console.WriteLine($"Doctor assignment completed in {stopwatch.ElapsedMilliseconds / 1000.0:F1} seconds");
            }

            // Step C: Merge the schedules
            mainSchedule = MergeSchedules(doctorSchedule, surgerySchedule);

            // Performance tracking
            stopwatch.Stop();
            Console.WriteLine($"\nTotal scheduling completed in {stopwatch.ElapsedMilliseconds / 1000.0:F2} seconds");

            // Print final schedule statistics
            GetFinalScheduleStatistics();

            return mainSchedule;
        }

        // NEW METHOD: Updates the original patients with surgeon assignments from the schedule
        private void UpdateOriginalPatientsWithSurgeons(Schedule schedule)
        {
            if (schedule?.SurgerySchedule == null)
                return;

            // Create a temporary dictionary to hold surgeon assignments (patientId -> surgeonId)
            Dictionary<int, int> surgeonAssignments = new Dictionary<int, int>();

            // Extract all surgeon assignments from schedule
            foreach (var dateEntry in schedule.SurgerySchedule)
            {
                foreach (var roomEntry in dateEntry.Value)
                {
                    var roomId = roomEntry.Key;
                    var patientIds = roomEntry.Value;

                    foreach (var patientId in patientIds)
                    {
                        // Try to find if this patient has a surgeon assigned in PatientToDoctor
                        if (schedule.PatientToDoctor.TryGetValue(patientId, out int surgeonId))
                        {
                            surgeonAssignments[patientId] = surgeonId;
                        }
                    }
                }
            }

            // Now update the original patient list with surgeon assignments
            foreach (var patient in patients)
            {
                if (surgeonAssignments.TryGetValue(patient.Id, out int surgeonId))
                {
                    Console.WriteLine($"Updating patient {patient.Id} with surgeon {surgeonId}");
                    patient.AssignedSurgeonId = surgeonId;
                }
            }

            // If PatientToDoctor doesn't contain the surgeon assignments, we need to look for them elsewhere
            // This is a backup method in case the primary approach doesn't work
            if (surgeonAssignments.Count == 0)
            {
                Console.WriteLine("Using fallback method for surgeon assignments");
                // Iterate through all days and rooms in the surgery schedule
                foreach (var dateEntry in schedule.SurgerySchedule)
                {
                    foreach (var roomEntry in dateEntry.Value)
                    {
                        foreach (var patientId in roomEntry.Value)
                        {
                            // Find this patient in the original list
                            var patient = patients.FirstOrDefault(p => p.Id == patientId);
                            if (patient != null)
                            {
                                // If surgeon not assigned yet, find a matching doctor by specialization
                                if (!patient.AssignedSurgeonId.HasValue)
                                {
                                    var surgeon = doctors.FirstOrDefault(d =>
                                        d.Specialization == patient.RequiredSpecialization);

                                    if (surgeon != null)
                                    {
                                        Console.WriteLine($"Fallback: Assigning surgeon {surgeon.Id} to patient {patient.Id}");
                                        patient.AssignedSurgeonId = surgeon.Id;

                                        // Also update the PatientToDoctor mapping in the schedule
                                        if (!schedule.PatientToDoctor.ContainsKey(patient.Id))
                                        {
                                            schedule.PatientToDoctor[patient.Id] = surgeon.Id;
                                        }

                                        // Update DoctorToPatients mapping
                                        if (!schedule.DoctorToPatients.ContainsKey(surgeon.Id))
                                        {
                                            schedule.DoctorToPatients[surgeon.Id] = new List<int>();
                                        }

                                        if (!schedule.DoctorToPatients[surgeon.Id].Contains(patient.Id))
                                        {
                                            schedule.DoctorToPatients[surgeon.Id].Add(patient.Id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Schedule MergeSchedules(Schedule doctorSchedule, Schedule surgerySchedule)
        {
            // Create a new merged schedule
            var mergedSchedule = new Schedule
            {
                DoctorToPatients = new Dictionary<int, List<int>>(),
                PatientToDoctor = new Dictionary<int, int>(),
                SurgerySchedule = new Dictionary<DateTime, Dictionary<int, List<int>>>()
            };

            // Copy doctor assignments if available
            if (doctorSchedule != null)
            {
                // Deep copy to avoid reference issues
                foreach (var doctorId in doctorSchedule.DoctorToPatients.Keys)
                {
                    mergedSchedule.DoctorToPatients[doctorId] = new List<int>(doctorSchedule.DoctorToPatients[doctorId]);
                }

                foreach (var patientId in doctorSchedule.PatientToDoctor.Keys)
                {
                    mergedSchedule.PatientToDoctor[patientId] = doctorSchedule.PatientToDoctor[patientId];
                }
            }

            // Copy surgery schedule if available
            if (surgerySchedule != null && surgerySchedule.SurgerySchedule != null)
            {
                foreach (var dateEntry in surgerySchedule.SurgerySchedule)
                {
                    var date = dateEntry.Key;
                    mergedSchedule.SurgerySchedule[date] = new Dictionary<int, List<int>>();

                    foreach (var roomEntry in dateEntry.Value)
                    {
                        var roomId = roomEntry.Key;
                        mergedSchedule.SurgerySchedule[date][roomId] = new List<int>(roomEntry.Value);

                        // Also update regular doctor assignments to include surgeons
                        foreach (var patientId in roomEntry.Value)
                        {
                            var patient = patients.FirstOrDefault(p => p.Id == patientId);
                            if (patient != null && patient.AssignedSurgeonId.HasValue)
                            {
                                int surgeonId = patient.AssignedSurgeonId.Value;

                                // Add surgeon-patient relationship
                                if (!mergedSchedule.DoctorToPatients.ContainsKey(surgeonId))
                                {
                                    mergedSchedule.DoctorToPatients[surgeonId] = new List<int>();
                                }

                                if (!mergedSchedule.DoctorToPatients[surgeonId].Contains(patientId))
                                {
                                    mergedSchedule.DoctorToPatients[surgeonId].Add(patientId);
                                }

                                mergedSchedule.PatientToDoctor[patientId] = surgeonId;
                            }
                        }
                    }
                }
            }

            return mergedSchedule;
        }

        private void GetFinalScheduleStatistics()
        {
            Console.WriteLine("\n=== Final Schedule Statistics ===");

            // Count total patients assigned
            totalAssigned = mainSchedule.PatientToDoctor.Count;
            double assignmentPercentage = (double)totalAssigned / patients.Count * 100;
            stat.totalAssigned = totalAssigned;
            stat.assignmentPercentage = assignmentPercentage;
            Console.WriteLine($"Total patients assigned: {totalAssigned}/{patients.Count} ({assignmentPercentage:F1}%)");

            // Doctor workload statistics
            if (mainSchedule.DoctorToPatients.Any())
            {
                var doctorWorkloads = mainSchedule.DoctorToPatients.Keys
                    .Select(id => {
                        var doctor = doctors.First(d => d.Id == id);
                        int patientCount = mainSchedule.DoctorToPatients[id].Count;
                        return new
                        {
                            DoctorId = id,
                            Name = doctor.Name,
                            Patients = patientCount,
                            MaxCapacity = doctor.MaxWorkload,
                            UtilizationPercentage = (double)patientCount / doctor.MaxWorkload * 100
                        };
                    })
                    .ToList();

                double avgWorkload = doctorWorkloads.Average(d => d.UtilizationPercentage);
                double minWorkload = doctorWorkloads.Min(d => d.UtilizationPercentage);
                double maxWorkload = doctorWorkloads.Max(d => d.UtilizationPercentage);

                Console.WriteLine($"Doctor workload - Min: {minWorkload:F1}%, Avg: {avgWorkload:F1}%, Max: {maxWorkload:F1}%");
                stat.AvarageDoctorWorkLoad = avgWorkload;

                // List top 5 busiest doctors
                Console.WriteLine("\nTop 5 busiest doctors:");
                foreach (var doc in doctorWorkloads.OrderByDescending(d => d.UtilizationPercentage).Take(5))
                {
                    Console.WriteLine($"  Dr. {doc.Name}: {doc.Patients}/{doc.MaxCapacity} patients ({doc.UtilizationPercentage:F1}%)");
                }
            }

            // Specialization match analysis
            AnalyzeSpecializationMatch();

            // Surgery schedule statistics
            if (mainSchedule.SurgerySchedule != null && mainSchedule.SurgerySchedule.Any())
            {
                totalSurgeries = 0;
                int totalRoomSlots = 0;

                foreach (var dateEntry in mainSchedule.SurgerySchedule)
                {
                    foreach (var roomEntry in dateEntry.Value)
                    {
                        totalRoomSlots++;
                        totalSurgeries += roomEntry.Value.Count;
                    }
                }

                double roomUtilization = totalRoomSlots > 0 ?
                    (double)totalSurgeries / totalRoomSlots * 100 : 0;

                int surgeryPatientCount = patients.Count(p => p.NeedsSurgery);
                double surgeryCompletionRate = surgeryPatientCount > 0 ?
                    (double)totalSurgeries / surgeryPatientCount * 100 : 0;

                Console.WriteLine($"\nSurgery Statistics:");
                Console.WriteLine($"  Total surgeries scheduled: {totalSurgeries}/{surgeryPatientCount} ({surgeryCompletionRate:F1}%)");
                Console.WriteLine($"  Operating room utilization: {roomUtilization:F1}%");

                // Print surgeries by day
                Console.WriteLine("\nSurgeries by day:");
                foreach (var dateEntry in mainSchedule.SurgerySchedule.OrderBy(d => d.Key))
                {
                    int dailySurgeries = dateEntry.Value.Sum(r => r.Value.Count);
                    if (dailySurgeries > 0)
                    {
                        Console.WriteLine($"  {dateEntry.Key.ToShortDateString()}: {dailySurgeries} surgeries");
                    }
                }
            }
        }

        private void AnalyzeSpecializationMatch()
        {
            int correctSpecializationCount = 0;
            int totalAssigned = mainSchedule.PatientToDoctor.Count;

            // Count by urgency level
            int[] assignedByUrgency = new int[4]; // 0=none, 1=low, 2=medium, 3=high
            int[] correctByUrgency = new int[4];

            foreach (var pair in mainSchedule.PatientToDoctor)
            {
                int patientId = pair.Key;
                int doctorId = pair.Value;

                var patient = patients.FirstOrDefault(p => p.Id == patientId);
                var doctor = doctors.FirstOrDefault(d => d.Id == doctorId);

                if (patient != null && doctor != null)
                {
                    int urgencyLevel = patient.GetUrgencyValue();
                    assignedByUrgency[urgencyLevel]++;

                    if (doctor.Specialization == patient.RequiredSpecialization)
                    {
                        correctSpecializationCount++;
                        correctByUrgency[urgencyLevel]++;
                    }
                }
            }

            double specializationMatchRate = totalAssigned > 0 ?
            (double)correctSpecializationCount / totalAssigned * 100 : 0;

            Console.WriteLine($"\nSpecialization Match:");
            Console.WriteLine($"  Overall: {correctSpecializationCount}/{totalAssigned} ({specializationMatchRate:F1}%)");
            stat.specializationMatchRate = specializationMatchRate;

            // Print by urgency
            for (int i = 1; i <= 3; i++)
            {
                string urgencyName = i == 1 ? "Low" : (i == 2 ? "Medium" : "High");
                double urgencyMatchRate = assignedByUrgency[i] > 0 ?
                    (double)correctByUrgency[i] / assignedByUrgency[i] * 100 : 0;

                Console.WriteLine($"  {urgencyName} urgency: {correctByUrgency[i]}/{assignedByUrgency[i]} ({urgencyMatchRate:F1}%)");
            }
        }
    }
}