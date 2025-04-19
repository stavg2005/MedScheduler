using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
// Assuming your Models (Doctor, Patient, Surgeon, OperatingRoom, MedicalProcedure, Enums) are in Models namespace
using Models;
// Assuming your Schedulers (DoctorScheduler, SurgeryScheduler, Schedule) are in SchedulingAlgorithm namespace
using ClassLibrary1;
using System.Collections.Concurrent;
using System.Reflection;
using DB;
// Assuming DataSingelton provides data (replace if using DataManager or direct DB access)
// using DB; // Or MedScheduler if DataSingelton/DataManager is there

namespace MedScheduler // Or your appropriate namespace
{
    /// <summary>
    /// Orchestrates the execution of different scheduling algorithms (Surgery and Doctor assignments).
    /// </summary>
    public class SchedulerOrchestrator
    {
        // Data fetched from source (e.g., DataSingelton, DataManager, Database)
        private readonly List<Doctor> allDoctors;
        private readonly List<Patient> allPatients;
        private readonly List<OperatingRoom> allOperatingRooms;
        private readonly List<MedicalProcedure> allProcedures; // Added

        public int GaPopulationSize { get; set; } = 300; // Default value
        public int GaMaxGenerations { get; set; } = 500; // Default value
        public double GaCrossoverRate { get; set; } = 0.85; // Default value
        public double GaMutationRate { get; set; } = 0.1; // Default value
        public int GaMaxStagnation { get; set; } = 100; // Default value
        public double GaFitnessThreshold { get; set; } = 100000; // Default value

        public double GaSpecializationMatchWeight { get; set; } = 6.0;
        public double GaUrgencyWeight { get; set; } = 4.5;
        public double GaWorkloadBalanceWeight { get; set; } = 5.5;
        public double GaPatientAssignmentWeight { get; set; } = 1.5;
        public double GaContinuityOfCareWeight { get; set; } = 2.0;
        public double GaHierarchyWeight { get; set; } = 2.5;
        public double GaExperienceLevelWeight { get; set; } = 3.0;
        public double GaPreferenceMatchWeight { get; set; } = 4.0;
        // Parameters for genetic algorithm
        public readonly int populationSize = 300; // Example value, tune as needed
        // public readonly int maxGenerations = 500; // Can be set on DoctorScheduler instance

        // For tracking performance
        public readonly Stopwatch stopwatch = new Stopwatch();

        private int gaGenerationsRun = 0; // Track actual generations run by GA
        private double gaFinalBestFitness = double.MinValue; // Track actual best fitness from GA
        // Results storage
        public Schedule finalDoctorSchedule; // Stores the result from DoctorScheduler
        public List<Patient> unscheduledSurgeryPatients; // Stores patients needing surgery that couldn't be scheduled
        public Statistics finalStatistics = new Statistics(); // Stores calculated stats
        private ConcurrentDictionary<int, int> previousAssignments = new ConcurrentDictionary<int, int>(); // Populated in constructor
        /// <summary>
        /// Initializes the orchestrator, fetching data from a source (e.g., DataSingelton).
        /// </summary>
        public SchedulerOrchestrator()
        {
            // --- Fetch data ---
            try
            {


                this.allDoctors = DataSingelton.Instance.Doctors ?? new List<Doctor>();
                this.allPatients = DataSingelton.Instance.Patients ?? new List<Patient>();
                this.allOperatingRooms = DataSingelton.Instance.OperatingRooms ?? new List<OperatingRoom>();
                this.allProcedures = DataSingelton.Instance.Procedures ?? new List<MedicalProcedure>();

                foreach (var patient in this.allPatients)
                {
                    if (patient.PreviousDoctors != null && patient.PreviousDoctors.Any())
                    {
                        previousAssignments[patient.Id] = patient.PreviousDoctors.Last();
                    }
                }

                // Basic validation
                if (!this.allDoctors.Any() || !this.allPatients.Any())
                {
                    Console.WriteLine("Warning: Doctor or Patient list is empty. Scheduling might not produce results.");
                    // Consider throwing an exception if this is critical
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL: Error initializing data for orchestrator: {ex.Message}");
                // Handle exception appropriately - maybe rethrow or ensure lists are empty
                this.allDoctors = new List<Doctor>();
                this.allPatients = new List<Patient>();
                this.allOperatingRooms = new List<OperatingRoom>();
                this.allProcedures = new List<MedicalProcedure>();
            }
        }

        /// <summary>
        /// Runs the surgery and doctor assignment scheduling processes.
        /// </summary>
        /// <returns>The final schedule object containing doctor assignments.</returns>
        /// 
        private async Task UpdateSingeletonData(Schedule bestSchedule)
        {

           foreach (var doctor in DataSingelton.Instance.Doctors)
            {
                if (bestSchedule.DoctorToPatients.TryGetValue(doctor.Id, out var assignedPatientIDs))
                {
                    
                    doctor.patientsIDS = assignedPatientIDs;
                }
                else
                {

                    doctor.patientsIDS = new List<int>(); 
                }

                doctor.SetCurrentWorkLoad();
            }


            UpdatePatientsWithDoctorAssignments(bestSchedule);

        }
        public async Task<Schedule> GenerateOptimalSchedule()
        {
            stopwatch.Restart(); // Use Restart instead of Start

            Console.WriteLine("\n=== Starting MedScheduler Optimization ===");
            Console.WriteLine($"Total doctors: {allDoctors.Count} ({allDoctors.OfType<Surgeon>().Count()} Surgeons)");
            Console.WriteLine($"Total patients: {allPatients.Count}");
            Console.WriteLine($"Total procedures: {allProcedures.Count}");
            Console.WriteLine($"Total operating rooms: {allOperatingRooms.Count}");

            // Step 1: Identify patients needing surgery vs. regular assignment
            // Ensure patients needing surgery also have a RequiredProcedureId
            var surgeryPatients = allPatients.Where(p => p.NeedsSurgery && p.RequiredProcedureId.HasValue && !p.ScheduledSurgeryDate.HasValue).ToList();
            var regularPatients = allPatients.Where(p => !p.NeedsSurgery).ToList(); // Patients for GA

            Console.WriteLine($"\nPatients needing surgery scheduling: {surgeryPatients.Count}");
            Console.WriteLine($"Patients needing doctor assignment: {regularPatients.Count}");

            // --- Step 2: Schedule Surgeries (Greedy Algorithm for a defined period) ---
            unscheduledSurgeryPatients = new List<Patient>();
            if (surgeryPatients.Any() && allOperatingRooms.Any() && allProcedures.Any() && allDoctors.OfType<Surgeon>().Any())
            {
                Console.WriteLine("\n=== Starting Surgery Scheduling (Greedy Algorithm) ===");

                // *** Define the scheduling period (e.g., next 4 weeks) ***
                DateTime periodStartDate = GetNextMonday(DateTime.Now.Date); // Start from next Monday
                DateTime periodEndDate = periodStartDate.AddDays(28); // Schedule for 4 weeks (exclusive end date)
                Console.WriteLine($"Scheduling surgeries from {periodStartDate:yyyy-MM-dd} to {periodEndDate.AddDays(-1):yyyy-MM-dd}");

                var surgeryScheduler = new SurgeryScheduler(
                    surgeryPatients,
                    allDoctors,
                    allOperatingRooms,
                    allProcedures,
                    periodStartDate, // Pass start date
                    periodEndDate    // Pass end date
                );

                unscheduledSurgeryPatients = surgeryScheduler.ScheduleSurgeries(); // Run monthly scheduler

                Console.WriteLine($"Surgery scheduling attempted.");
                Console.WriteLine($"  Successfully scheduled: {surgeryPatients.Count(p => p.ScheduledSurgeryDate.HasValue)}");
                Console.WriteLine($"  Could not schedule: {unscheduledSurgeryPatients.Count}");
                Console.WriteLine($"Surgery scheduling completed in {stopwatch.ElapsedMilliseconds / 1000.0:F1} seconds");
            }
            else
            {
                Console.WriteLine("\nSkipping surgery scheduling (missing required data/resources).");
                unscheduledSurgeryPatients.AddRange(surgeryPatients);
            }

            // --- Step 3: Assign Regular Patients (Genetic Algorithm) ---
            finalDoctorSchedule = null; // Initialize
            if (regularPatients.Any() && allDoctors.Any())
            {
                Console.WriteLine("\n=== Starting Doctor Assignment (Genetic Algorithm) ===");
                Console.WriteLine($"GA Params: PopSize={GaPopulationSize}, MaxGen={GaMaxGenerations}, CrossRate={GaCrossoverRate}, MutRate={GaMutationRate}");
                var geneticScheduler = new DoctorScheduler(
                    populationSize,
                    allDoctors,
                    regularPatients // Pass only patients needing doctor assignment
                );

                // Set parameters on the GA instance
                geneticScheduler.maxGenerations = this.GaMaxGenerations;
                geneticScheduler.maxStagnation = this.GaMaxStagnation;
                geneticScheduler.fitnessThreshold = this.GaFitnessThreshold;
                // Set weights (using reflection or direct field access if made internal/public)
                SetSchedulerParameters(geneticScheduler); // Use helper to set weights
                // Execute the genetic algorithm
                // Using Task.Run to avoid blocking UI thread if called from one, though this method is async Task
                finalDoctorSchedule = await Task.Run(() => geneticScheduler.Solve());
                this.gaGenerationsRun = geneticScheduler.currentGeneration; // Store actual generations
                this.gaFinalBestFitness = geneticScheduler.bestFitness; // Store actual fitness
                // Update the main patient list with doctor assignments from the final schedule
                await UpdateSingeletonData(finalDoctorSchedule);

                Console.WriteLine($"Doctor assignment completed in {stopwatch.ElapsedMilliseconds / 1000.0:F1} seconds");
            }
            else
            {
                Console.WriteLine("\nSkipping doctor assignment (no regular patients or doctors available).");
                finalDoctorSchedule = new Schedule(); // Assign empty schedule
            }


            // --- Step 4: Finalization and Statistics ---
            stopwatch.Stop();
            Console.WriteLine($"\nTotal scheduling process completed in {stopwatch.ElapsedMilliseconds / 1000.0:F2} seconds");

            // Calculate and print final statistics based on the state of 'allPatients' and 'finalDoctorSchedule'
           finalStatistics = CalculateAndPrintFinalStatistics();
            PrintStatistics(finalStatistics);
            // Return the schedule containing doctor assignments
            // Surgery assignments are reflected in the 'allPatients' list properties
            return finalDoctorSchedule;
        }
        /// <summary>
        /// Helper to set fitness weights on the DoctorScheduler instance.
        /// Assumes fields exist in DoctorScheduler. Consider making them internal properties.
        /// </summary>
        private void SetSchedulerParameters(DoctorScheduler scheduler)
        {
            try
            {
                // Use reflection to set private readonly fields (adjust if they become properties)
                var type = typeof(DoctorScheduler);
                SetFieldValue(scheduler, type, "specializationMatchWeight", this.GaSpecializationMatchWeight);
                SetFieldValue(scheduler, type, "urgencyWeight", this.GaUrgencyWeight);
                SetFieldValue(scheduler, type, "workloadBalanceWeight", this.GaWorkloadBalanceWeight);
                SetFieldValue(scheduler, type, "patientAssignmentWeight", this.GaPatientAssignmentWeight);
                SetFieldValue(scheduler, type, "continuityOfCareWeight", this.GaContinuityOfCareWeight);
                SetFieldValue(scheduler, type, "hierarchyWeight", this.GaHierarchyWeight);
                SetFieldValue(scheduler, type, "experienceLevelWeight", this.GaExperienceLevelWeight);
                SetFieldValue(scheduler, type, "preferenceMatchWeight", this.GaPreferenceMatchWeight);

                // Also set rates if they are private fields
                SetFieldValue(scheduler, type, "crossoverRate", this.GaCrossoverRate);
                SetFieldValue(scheduler, type, "mutationRate", this.GaMutationRate);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set GA parameters via reflection. Using defaults. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper to set a field value using reflection.
        /// </summary>
        private void SetFieldValue(object target, Type type, string fieldName, object value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, Convert.ChangeType(value, field.FieldType));
            }
            else
            {
                Console.WriteLine($"Warning: Field '{fieldName}' not found on {type.Name} for setting parameters.");
            }
        }

        /// <summary>
        /// Updates the main list of patients with the doctor assignments from the final GA schedule.
        /// Avoids overwriting surgeon assignments made by the SurgeryScheduler.
        /// </summary>
        private void UpdatePatientsWithDoctorAssignments(Schedule schedule)
        {
            if (schedule == null || schedule.PatientToDoctor == null || allPatients == null) return;

            var patientLookup = allPatients.ToDictionary(p => p.Id);

            // Assign doctors based on the schedule
            foreach (var kvp in schedule.PatientToDoctor)
            {
                if (patientLookup.TryGetValue(kvp.Key, out Patient patient))
                {
                    // Assign doctor from GA ONLY IF the patient doesn't need surgery OR
                    // if they do need surgery but haven't been assigned a surgeon yet by the SurgeryScheduler.
                    if (!patient.NeedsSurgery || !patient.AssignedSurgeonId.HasValue)
                    {
                        patient.AssignedDoctorId = kvp.Value;
                    }
                    // If patient.NeedsSurgery AND patient.AssignedSurgeonId.HasValue, we keep the surgeon assignment
                    // and do NOT assign a general doctor from the GA (unless specific rules dictate otherwise).
                    // You could potentially assign the surgeon as the main doctor too if needed:
                    // else if (patient.NeedsSurgery && patient.AssignedSurgeonId.HasValue) { patient.AssignedDoctorId = patient.AssignedSurgeonId; }
                }
            }

            // Clear assignments for regular patients *not* in the final schedule
            foreach (var patient in allPatients)
            {
                // Clear only if it's a regular patient OR a surgery patient without a surgeon assigned
                if (!patient.NeedsSurgery || !patient.AssignedSurgeonId.HasValue)
                {
                    if (!schedule.PatientToDoctor.ContainsKey(patient.Id))
                    {
                        patient.AssignedDoctorId = null;
                    }
                }
            }
        }


        /// <summary>
        /// Calculates and prints final statistics based on the generated schedules.
        /// </summary>
        private Statistics CalculateAndPrintFinalStatistics()
        {
            Console.WriteLine("\n=== Final Schedule Statistics ===");
            var stats = new Statistics();
            var schedule = finalDoctorSchedule ?? new Schedule(); // Use empty schedule if GA didn't run/produce result
            var assignments = schedule.PatientToDoctor ?? new Dictionary<int, int>();

            Console.WriteLine("[CalculateFinalStatistics Debug]");
            Console.WriteLine($"  Input: finalDoctorSchedule is null? {finalDoctorSchedule == null}");
            Console.WriteLine($"  Input: Number of GA assignments (PatientToDoctor.Count): {assignments.Count}");
            Console.WriteLine($"  Input: Total Patients: {allPatients.Count}");
            int totalRegular = allPatients.Count(p => !p.NeedsSurgery);
            int totalSurgery = allPatients.Count(p => p.NeedsSurgery);
            int scheduledSurgery = allPatients.Count(p => p.NeedsSurgery && p.ScheduledSurgeryDate.HasValue);
            Console.WriteLine($"  Input: Total Regular Patients: {totalRegular}");
            Console.WriteLine($"  Input: Total Surgery Patients: {totalSurgery}");
            Console.WriteLine($"  Input: Actually Scheduled Surgeries: {scheduledSurgery}");
            // --- Performance ---
            stats.TotalElapsedTimeSeconds = stopwatch.ElapsedMilliseconds / 1000.0;
            stats.GaGenerations = this.gaGenerationsRun;
            stats.GaFinalFitness = this.gaFinalBestFitness;

            // --- Patient Counts ---
            stats.TotalRegularPatients = allPatients.Count(p => !p.NeedsSurgery);
            stats.TotalSurgeryPatients = allPatients.Count(p => p.NeedsSurgery);

            // --- Doctor Assignment Stats ---
            stats.AssignedRegularPatients = assignments.Count(kvp =>
                allPatients.Any(p => p.Id == kvp.Key && !p.NeedsSurgery));

            stats.RegularPatientAssignmentPercentage = stats.TotalRegularPatients > 0
                ? (double)stats.AssignedRegularPatients / stats.TotalRegularPatients * 100
                : 0;

            // --- Workload Stats ---
            var doctorWorkloads = new Dictionary<int, int>();
            var doctorUtilization = new List<double>();
            if (assignments.Any())
            {
                // Calculate workload based *only* on GA schedule assignments
                doctorWorkloads = schedule.DoctorToPatients
                                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);

                foreach (var doctor in allDoctors)
                {
                    int load = doctorWorkloads.TryGetValue(doctor.Id, out int count) ? count : 0;
                    if (doctor.MaxWorkload > 0)
                    {
                        doctorUtilization.Add((double)load / doctor.MaxWorkload * 100);
                    }
                    else if (load > 0) // Has patients but no max workload defined? Treat as 100%?
                    {
                        doctorUtilization.Add(100.0);
                    }
                    // else: doctor has 0 max workload and 0 patients assigned -> skip for utilization stats
                }
            }

            stats.AverageDoctorWorkloadPercent = doctorUtilization.Any() ? doctorUtilization.Average() : 0;
            stats.MinDoctorWorkloadPercent = doctorUtilization.Any() ? doctorUtilization.Min() : 0;
            stats.MaxDoctorWorkloadPercent = doctorUtilization.Any() ? doctorUtilization.Max() : 0;
            if (doctorUtilization.Count > 1)
            {
                double avg = stats.AverageDoctorWorkloadPercent;
                double sumOfSquares = doctorUtilization.Sum(u => Math.Pow(u - avg, 2));
                stats.StdDevDoctorWorkloadPercent = Math.Sqrt(sumOfSquares / doctorUtilization.Count);
            }
            else
            {
                stats.StdDevDoctorWorkloadPercent = 0;
            }


            // --- Quality Metrics (based on GA schedule assignments) ---
            int specMatchCount = 0;
            int continuityMatchCount = 0;
            int experienceMatchCount = 0;
            double totalPreferenceScore = 0;
            int assignmentsAnalyzedForQuality = 0; // Count assignments checked for these metrics
            int patientsWithPreviousDoctor = 0; // Count how many assigned patients had a previous doctor

            foreach (var pair in assignments)
            {
                int patientId = pair.Key;
                int doctorId = pair.Value;

                var patient = allPatients.FirstOrDefault(p => p.Id == patientId);
                var doctor = allDoctors.FirstOrDefault(d => d.Id == doctorId);

                // Analyze only if patient/doctor found and it's a non-surgery assignment
                if (patient != null && doctor != null && !patient.NeedsSurgery)
                {
                    assignmentsAnalyzedForQuality++;

                    // Specialization
                    if (doctor.Specialization == patient.RequiredSpecialization)
                    {
                        specMatchCount++;
                    }

                    // Experience
                    ExperienceLevel requiredLevel = GetRequiredExperienceLevel(patient.Urgency);
                    if (doctor.ExperienceLevel >= requiredLevel)
                    {
                        experienceMatchCount++;
                    }

                    // Continuity
                    if (previousAssignments.TryGetValue(patientId, out int prevDocId))
                    {
                        patientsWithPreviousDoctor++; // This assigned patient had a history
                        if (prevDocId == doctorId)
                        {
                            continuityMatchCount++;
                        }
                    }

                    // Preference (using helper from DoctorScheduler context)
                    // Need access to the CalculatePreferenceScoreForPair method or replicate it here
                    totalPreferenceScore += CalculatePreferenceScoreForPair(doctor, patient); // Assumes this method is accessible/replicated
                }
            }

            stats.SpecializationMatchRatePercent = assignmentsAnalyzedForQuality > 0
                ? (double)specMatchCount / assignmentsAnalyzedForQuality * 100 : 0;

            stats.ExperienceMatchRatePercent = assignmentsAnalyzedForQuality > 0
                ? (double)experienceMatchCount / assignmentsAnalyzedForQuality * 100 : 0;

            // Calculate continuity based on those who *had* a previous doctor
            stats.ContinuityOfCareRatePercent = patientsWithPreviousDoctor > 0
                ? (double)continuityMatchCount / patientsWithPreviousDoctor * 100 : 0;

            stats.AveragePreferenceScore = assignmentsAnalyzedForQuality > 0
                ? totalPreferenceScore / assignmentsAnalyzedForQuality : 0.5; // Default to neutral if none analyzed


            // --- Surgery Stats ---
            stats.ScheduledSurgeriesCount = allPatients.Count(p => p.NeedsSurgery && p.ScheduledSurgeryDate.HasValue);
            stats.SurgeryCompletionRatePercent = stats.TotalSurgeryPatients > 0
                ? (double)stats.ScheduledSurgeriesCount / stats.TotalSurgeryPatients * 100
                : 0;

            return stats;
        }

        private void PrintStatistics(Statistics stats)
        {
            if (stats == null) return;

            Console.WriteLine("\n--- Final Schedule Statistics ---");
            Console.WriteLine("[Performance]");
            Console.WriteLine($"  Total Time: {stats.TotalElapsedTimeSeconds:F2} seconds");
            Console.WriteLine($"  GA Generations: {stats.GaGenerations}");
            Console.WriteLine($"  GA Final Fitness: {stats.GaFinalFitness:F2}");

            Console.WriteLine("\n[Doctor Assignments (Regular Patients)]");
            Console.WriteLine($"  Total Regular Patients: {stats.TotalRegularPatients}");
            Console.WriteLine($"  Assigned: {stats.AssignedRegularPatients} ({stats.RegularPatientAssignmentPercentage:F1}%)");
            Console.WriteLine($"  Workload (% Max): Avg={stats.AverageDoctorWorkloadPercent:F1}%, Min={stats.MinDoctorWorkloadPercent:F1}%, Max={stats.MaxDoctorWorkloadPercent:F1}%, StdDev={stats.StdDevDoctorWorkloadPercent:F1}%");
            Console.WriteLine($"  Specialization Match: {stats.SpecializationMatchRatePercent:F1}%");
            Console.WriteLine($"  Experience Level Match: {stats.ExperienceMatchRatePercent:F1}%");
            Console.WriteLine($"  Continuity of Care Match: {stats.ContinuityOfCareRatePercent:F1}%");
            Console.WriteLine($"  Average Preference Score: {stats.AveragePreferenceScore:F2} (0.0-1.0)");


            Console.WriteLine("\n[Surgery Assignments]");
            Console.WriteLine($"  Total Surgery Patients: {stats.TotalSurgeryPatients}");
            Console.WriteLine($"  Scheduled: {stats.ScheduledSurgeriesCount} ({stats.SurgeryCompletionRatePercent:F1}%)");
            Console.WriteLine("------------------------------------");
        }


        /// <summary>
        /// Calculates the preference match score (0 to 1) for a single doctor-patient pair.
        /// (Copied/adapted from DoctorScheduler - consider placing in a shared utility class)
        /// </summary>
        private double CalculatePreferenceScoreForPair(Doctor doctor, Patient patient)
        {
            if (doctor.Preferences == null || !doctor.Preferences.Any()) return 0.5; // Neutral

            double totalScore = 0;
            int relevantPreferenceCount = 0;

            foreach (var preference in doctor.Preferences)
            {
                double currentPrefScore = -1;
                try
                {
                    if (preference.Type == PreferenceType.PatientComplexity && preference.LevelValue.HasValue)
                    {
                        relevantPreferenceCount++;
                        ComplexityLevel prefLevel = (ComplexityLevel)preference.LevelValue.Value;
                        if (preference.Direction == PreferenceDirection.Prefers) currentPrefScore = (patient.ComplexityLevel == prefLevel) ? 1.0 : 0.2;
                        else currentPrefScore = (patient.ComplexityLevel == prefLevel) ? 0.0 : 0.8;
                    }
                    else if (preference.Type == PreferenceType.PatientUrgency && preference.LevelValue.HasValue)
                    {
                        relevantPreferenceCount++;
                        UrgencyLevel prefLevel = (UrgencyLevel)preference.LevelValue.Value;
                        if (preference.Direction == PreferenceDirection.Prefers) currentPrefScore = (patient.Urgency == prefLevel) ? 1.0 : 0.2;
                        else currentPrefScore = (patient.Urgency == prefLevel) ? 0.0 : 0.8;
                    }
                    else if (preference.Type == PreferenceType.PatientCondition && !string.IsNullOrEmpty(preference.ConditionValue))
                    {
                        relevantPreferenceCount++;
                        bool conditionMatch = (patient.Condition != null && patient.Condition.Equals(preference.ConditionValue, StringComparison.OrdinalIgnoreCase));
                        if (preference.Direction == PreferenceDirection.Prefers) currentPrefScore = conditionMatch ? 1.0 : 0.2;
                        else currentPrefScore = conditionMatch ? 0.0 : 0.8;
                    }
                }
                catch { currentPrefScore = 0.5; if (relevantPreferenceCount == 0) relevantPreferenceCount = 1; } // Handle errors, avoid div by zero

                if (currentPrefScore >= 0) totalScore += currentPrefScore;
            }
            return (relevantPreferenceCount == 0) ? 0.5 : totalScore / relevantPreferenceCount;
        }

        /// <summary>
        /// Helper to determine required experience based on patient urgency enum.
        /// (Copied/adapted from DoctorScheduler - consider placing in a shared utility class)
        /// </summary>
        private ExperienceLevel GetRequiredExperienceLevel(UrgencyLevel urgency)
        {
            switch (urgency)
            {
                case UrgencyLevel.High: return ExperienceLevel.Senior;
                case UrgencyLevel.Medium: return ExperienceLevel.Regular;
                default: return ExperienceLevel.Junior;
            }
        }


        /// <summary>
        /// Analyzes the specialization match rate based on the doctor assignment schedule from GA.
        /// </summary>
        private void AnalyzeSpecializationMatch(Schedule schedule)
        {
            if (schedule == null || schedule.PatientToDoctor == null || !schedule.PatientToDoctor.Any())
            {
                Console.WriteLine("\nSpecialization Match: No GA assignments to analyze.");
                return;
            }

            int correctSpecializationCount = 0;
            int totalAssignmentsAnalyzed = 0;

            // Count by urgency level
            int[] assignedByUrgency = new int[4]; // 0=none, 1=low, 2=medium, 3=high
            int[] correctByUrgency = new int[4];

            foreach (var pair in schedule.PatientToDoctor)
            {
                int patientId = pair.Key;
                int doctorId = pair.Value;

                // Find corresponding objects (handle potential missing data)
                var patient = allPatients.FirstOrDefault(p => p.Id == patientId);
                var doctor = allDoctors.FirstOrDefault(d => d.Id == doctorId);

                // Only analyze if both found and patient didn't need surgery
                // (as GA primarily handles non-surgery patients)
                if (patient != null && doctor != null && !patient.NeedsSurgery)
                {
                    totalAssignmentsAnalyzed++;
                    int urgencyLevel = (int)patient.Urgency; // Use Enum value
                    if (urgencyLevel >= 1 && urgencyLevel <= 3) // Basic bounds check for array index
                    {
                        assignedByUrgency[urgencyLevel]++;
                    }


                    if (doctor.Specialization == patient.RequiredSpecialization)
                    {
                        correctSpecializationCount++;
                        if (urgencyLevel >= 1 && urgencyLevel <= 3)
                        {
                            correctByUrgency[urgencyLevel]++;
                        }
                    }
                }
            }

            double specializationMatchRate = totalAssignmentsAnalyzed > 0 ?
            (double)correctSpecializationCount / totalAssignmentsAnalyzed * 100 : 0;

            Console.WriteLine($"\nSpecialization Match (GA Assignments for Regular Patients):");
            Console.WriteLine($"  Overall: {correctSpecializationCount}/{totalAssignmentsAnalyzed} ({specializationMatchRate:F1}%)");
            finalStatistics.specializationMatchRate = specializationMatchRate; // Store overall rate

            // Print by urgency
            for (int i = 1; i <= 3; i++)
            {
                string urgencyName = ((UrgencyLevel)i).ToString(); // Get name from Enum
                double urgencyMatchRate = assignedByUrgency[i] > 0 ?
                    (double)correctByUrgency[i] / assignedByUrgency[i] * 100 : 0;

                Console.WriteLine($"  {urgencyName} urgency: {correctByUrgency[i]}/{assignedByUrgency[i]} ({urgencyMatchRate:F1}%)");
            }
        }

        /// <summary>
        /// Helper method to get the date of the next Monday from the given date.
        /// If the given date is Monday, it returns the following Monday.
        /// </summary>
        private DateTime GetNextMonday(DateTime date)
        {
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
            // If today is Monday (daysUntilMonday is 0), add 7 days to get next Monday.
            // Otherwise, add the calculated daysUntilMonday.
            return date.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);
        }

    }

    /// <summary>
    /// Helper class to store calculated statistics (Example structure).
    /// </summary>
    public class Statistics
    {
        public int totalAssigned { get; set; } // Regular patients assigned by GA
        public double assignmentPercentage { get; set; } // Percentage of regular patients assigned
        public double AvarageDoctorWorkLoad { get; set; } // Average % workload for doctors based on GA assignments
        public double specializationMatchRate { get; set; } // % match for GA assignments
                                                            // Add surgery stats if needed (e.g., total scheduled, completion rate)
        public int TotalSurgeriesScheduled { get; set; }
        public double SurgeryCompletionRate { get; set; }

        public int TotalRegularPatients { get; set; }
        public int AssignedRegularPatients { get; set; }
        public double RegularPatientAssignmentPercentage { get; set; }
        public double AverageDoctorWorkloadPercent { get; set; }
        public double MinDoctorWorkloadPercent { get; set; }
        public double MaxDoctorWorkloadPercent { get; set; }
        public double StdDevDoctorWorkloadPercent { get; set; }
        public double SpecializationMatchRatePercent { get; set; }
        public double ContinuityOfCareRatePercent { get; set; } // New
        public double ExperienceMatchRatePercent { get; set; } // New
        public double AveragePreferenceScore { get; set; } // New (0.0 to 1.0)

        // Surgery Assignments
        public int TotalSurgeryPatients { get; set; }
        public int ScheduledSurgeriesCount { get; set; }
        public double SurgeryCompletionRatePercent { get; set; }

        // Performance
        public double TotalElapsedTimeSeconds { get; set; }
        public int GaGenerations { get; set; } // Get from orchestrator.currentGeneration
        public double GaFinalFitness { get; set; } //
    }
}
