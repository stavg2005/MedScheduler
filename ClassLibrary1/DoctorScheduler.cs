using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class DoctorScheduler
    {
        #region Data Members
        // Core collections
        private List<Schedule> Population { get; set; }
        private readonly int populationSize;
        private readonly Random rnd = new Random();

        // Resource collections - using concurrent dictionary for thread safety
        private ConcurrentDictionary<int, Doctor> DoctorsById { get; set; }
        private ConcurrentDictionary<int, Patient> PatientsById { get; set; }

        // Sorted collections for efficient lookup
        private SortedDictionary<string, List<Doctor>> DoctorsBySpecialization { get; set; }
        private SortedDictionary<int, List<Patient>> PatientsByUrgency { get; set; }

        // Termination condition variables
        public  int maxGenerations = 500;
        public  int currentGeneration = 0;
        public  double fitnessThreshold = 100000;
        public  int stagnationCount = 0;
        public int maxStagnation = 100;
        public  double bestFitness = double.MinValue;
        public  double previousBestFitness = double.MinValue;

        // Genetic algorithm parameters
        private readonly double crossoverRate = 0.8;
        private readonly double mutationRate = 0.4;
        private readonly int tournamentSize = 7;
        private readonly int elitismCount;

        // Weights for fitness function
        private readonly double specializationMatchWeight = 6;
        private readonly double urgencyWeight = 4.5;
        private readonly double workloadBalanceWeight = 5.5;
        private readonly double patientAssignmentWeight = 1.5;
        private readonly double continuityOfCareWeight = 2;
        private readonly double hierarchyWeight = 2.5;
        private readonly double experienceLevelWeight = 3.0;

        // For tracking assignments for continuity of care
        private ConcurrentDictionary<int, int> previousAssignments = new ConcurrentDictionary<int, int>();
        #endregion

        #region Constructor
        public DoctorScheduler(int populationSize, List<Doctor> doctors, List<Patient> patients)
        {
            this.populationSize = populationSize;
            this.elitismCount = (int)(populationSize * 0.1); // 10% elitism

            // Initialize concurrent dictionaries
            DoctorsById = new ConcurrentDictionary<int, Doctor>();
            PatientsById = new ConcurrentDictionary<int, Patient>();

            // Initialize sorted dictionaries
            DoctorsBySpecialization = new SortedDictionary<string, List<Doctor>>();
            PatientsByUrgency = new SortedDictionary<int, List<Patient>>(Comparer<int>.Create((a, b) => b.CompareTo(a))); // Descending order

            // Populate the dictionaries
            PopulateDataStructures(doctors, patients);
        }
        #endregion

        #region Data Structure Setup
        private void PopulateDataStructures(List<Doctor> doctors, List<Patient> patients)
        {
            // Populate DoctorsById
            foreach (var doctor in doctors)
            {
                DoctorsById[doctor.Id] = doctor;

                // Initialize workload to zero
                doctor.Workload = 0;

                // Populate DoctorsBySpecialization
                if (!DoctorsBySpecialization.ContainsKey(doctor.Specialization))
                {
                    DoctorsBySpecialization[doctor.Specialization] = new List<Doctor>();
                }
                DoctorsBySpecialization[doctor.Specialization].Add(doctor);
            }

            // Populate PatientsById and PatientsByUrgency
            foreach (var patient in patients)
            {
                PatientsById[patient.Id] = patient;

                int urgencyValue = patient.GetUrgencyValue();
                if (!PatientsByUrgency.ContainsKey(urgencyValue))
                {
                    PatientsByUrgency[urgencyValue] = new List<Patient>();
                }
                PatientsByUrgency[urgencyValue].Add(patient);

                // If patient has a previous doctor assignment, add to previousAssignments
                if (patient.PreviousDoctors.Any())
                {
                    // Use the most recent doctor for continuity
                    previousAssignments[patient.Id] = patient.PreviousDoctors.Last();
                }
            }
        }
        #endregion

        #region Main Algorithm
        public Schedule Solve()
        {
            // Initialize the population with random solutions
            Population = GeneratePopulation(populationSize);

            Console.WriteLine("Initial population generated.");
            Console.WriteLine($"Best initial fitness: {Population.Max(ScoreSchedule)}");

            // Main evolutionary loop
            while (!TerminationConditionMet())
            {
                currentGeneration++;

                // Create a new generation
                List<Schedule> newPopulation = new List<Schedule>();

                // Elitism: Keep the best solutions
                newPopulation.AddRange(Population.OrderByDescending(ScoreSchedule).Take(elitismCount));

                // Fill the rest with new offspring
                while (newPopulation.Count < populationSize)
                {
                    // Parent selection using tournament selection
                    Schedule parent1 = TournamentSelection();
                    Schedule parent2 = TournamentSelection();

                    // Crossover with probability
                    List<Schedule> offspring;
                    if (rnd.NextDouble() < crossoverRate)
                    {
                        offspring = Crossover(parent1, parent2);
                    }
                    else
                    {
                        // If no crossover, clone parents
                        offspring = new List<Schedule> { CloneSchedule(parent1), CloneSchedule(parent2) };
                    }

                    // Mutation with probability
                    foreach (var child in offspring)
                    {
                        if (rnd.NextDouble() < mutationRate)
                        {
                            Mutate(child);
                        }

                        // Add to new population if there's space
                        if (newPopulation.Count < populationSize)
                        {
                            newPopulation.Add(child);
                        }
                    }
                }

                // Replace the old population
                Population = newPopulation;

                // Track the best fitness
                double currentBestFitness = Population.Max(ScoreSchedule);

                // Update stagnation counter
                if (currentBestFitness > bestFitness)
                {
                    previousBestFitness = bestFitness;
                    bestFitness = currentBestFitness;
                    stagnationCount = 0;

                    Console.WriteLine($"Generation {currentGeneration}: Improvement found! Best fitness: {bestFitness}");

                    // Print best schedule details periodically
                    
                    
                        PrintScheduleDetails(GetLeadingSchedule());
                    
                }
                else
                {
                    stagnationCount++;
                    Console.WriteLine($"Generation {currentGeneration}: No improvement. Stagnation count: {stagnationCount}");
                }
            }

            Console.WriteLine($"Genetic algorithm terminated after {currentGeneration} generations");
            Console.WriteLine($"Final best fitness: {bestFitness}");

            // Update previous assignments for continuity of care in future runs
            var finalSchedule = GetLeadingSchedule();
            foreach (var pair in finalSchedule.PatientToDoctor)
            {
                previousAssignments[pair.Key] = pair.Value;
            }

            return finalSchedule;
        }
        #endregion

        #region Population Generation
        private List<Schedule> GeneratePopulation(int size)
        {
            List<Schedule> population = new List<Schedule>(size);

            // Create multiple schedules in parallel for better performance
            Parallel.For(0, size, i =>
            {
                var schedule = GenerateRandomSchedule();
                lock (population)
                {
                    population.Add(schedule);
                }
            });

            return population;
        }

        private Schedule GenerateRandomSchedule()
        {
            var schedule = new Schedule
            {
                DoctorToPatients = new Dictionary<int, List<int>>(),
                PatientToDoctor = new Dictionary<int, int>()
            };

            // Reset workloads
            foreach (var doctor in DoctorsById.Values)
            {
                doctor.Workload = 0;
            }

            // CHANGE: Introduce more randomness in initial assignments
            var patients = PatientsById.Values
                .Where(p => !p.NeedsSurgery)
                .OrderBy(_ => rnd.Next()) // Fully randomized order
                .ToList();

            // CHANGE: Only apply optimizations to a portion of patients (e.g., 40%)
            int optimizationCutoff = (int)(patients.Count * 0.4);

            for (int i = 0; i < patients.Count; i++)
            {
                var patient = patients[i];

                // Only apply optimization criteria for higher-priority patients
                if (i < optimizationCutoff && patient.GetUrgencyValue() == 3) // Only high urgency
                {
                    // Apply your existing optimization logic for these
                    // [your existing logic here]
                }
                else
                {
                    // Completely random assignment for others
                    var availableDoctors = DoctorsById.Values
                        .Where(d => d.Workload < d.MaxWorkload)
                        .OrderBy(_ => rnd.Next()) // Random order
                        .ToList();

                    if (availableDoctors.Any())
                    {
                        AssignPatientToDoctor(schedule, patient.Id, availableDoctors.First().Id);
                    }
                }
            }

            return schedule;
        }
        #endregion

        #region Selection Methods
        private Schedule TournamentSelection()
        {
            // Select random schedules for the tournament
            var tournament = Enumerable.Range(0, tournamentSize)
                .Select(_ => Population[rnd.Next(Population.Count)])
                .ToList();

            // Return the best schedule from the tournament
            return tournament
                .OrderByDescending(ScoreSchedule)
                .First();
        }

        private Schedule GetLeadingSchedule()
        {
            return Population
                .OrderByDescending(ScoreSchedule)
                .FirstOrDefault();
        }
        #endregion

        #region Crossover Implementation
        private List<Schedule> Crossover(Schedule parent1, Schedule parent2)
        {
            // Create two offspring
            var offspring1 = new Schedule
            {
                DoctorToPatients = new Dictionary<int, List<int>>(),
                PatientToDoctor = new Dictionary<int, int>()
            };

            var offspring2 = new Schedule
            {
                DoctorToPatients = new Dictionary<int, List<int>>(),
                PatientToDoctor = new Dictionary<int, int>()
            };

            // Reset doctor workloads
            foreach (var doctor in DoctorsById.Values)
            {
                doctor.Workload = 0;
            }

            // Get all patients that have been assigned in either parent
            var allAssignedPatients = parent1.PatientToDoctor.Keys
                .Union(parent2.PatientToDoctor.Keys)
                .OrderBy(id => PatientsById[id].GetUrgencyValue())
                .ThenBy(_ => rnd.Next()) // Random order for same urgency
                .ToList();

            // Crossover point - can vary for different offspring
            int crossoverPoint = rnd.Next(allAssignedPatients.Count);

            for (int i = 0; i < allAssignedPatients.Count; i++)
            {
                int patientId = allAssignedPatients[i];

                // For offspring1: take from parent1 up to crossover point, then from parent2
                TryCrossoverAssignment(parent1, parent2, offspring1, patientId, i < crossoverPoint);

                // For offspring2: take from parent2 up to crossover point, then from parent1
                TryCrossoverAssignment(parent2, parent1, offspring2, patientId, i < crossoverPoint);
            }

            // Try to assign remaining patients
            AssignRemainingPatients(offspring1);
            AssignRemainingPatients(offspring2);

            return new List<Schedule> { offspring1, offspring2 };
        }

        private void TryCrossoverAssignment(Schedule primaryParent, Schedule secondaryParent, Schedule offspring, int patientId, bool useFirstParent)
        {
            Schedule sourceParent = useFirstParent ? primaryParent : secondaryParent;

            if (sourceParent.PatientToDoctor.TryGetValue(patientId, out int doctorId))
            {
                // Check if this doctor can still take on patients
                if (DoctorsById.TryGetValue(doctorId, out Doctor doctor) && doctor.Workload < doctor.MaxWorkload)
                {
                    AssignPatientToDoctor(offspring, patientId, doctorId);
                }
                else if (!useFirstParent && primaryParent.PatientToDoctor.TryGetValue(patientId, out int altDoctorId))
                {
                    // Try alternative from other parent as fallback
                    if (DoctorsById.TryGetValue(altDoctorId, out Doctor altDoctor) && altDoctor.Workload < altDoctor.MaxWorkload)
                    {
                        AssignPatientToDoctor(offspring, patientId, altDoctorId);
                    }
                }
            }
        }
        #endregion

        #region Mutation Implementation
        private void Mutate(Schedule schedule)
        {
            // Pick a mutation strategy based on the schedule's state
            int mutationType = rnd.Next(4);

            switch (mutationType)
            {
                case 0: // Reassign patients to better match specialization
                    MutateForSpecialization(schedule);
                    break;
                case 1: // Balance workloads
                    MutateForWorkloadBalance(schedule);
                    break;
                case 2: // Improve continuity of care
                    MutateForContinuity(schedule);
                    break;
                case 3: // Add unassigned patients 
                    MutateAddUnassignedPatients(schedule);
                    break;
            }
        }

        private void MutateForSpecialization(Schedule schedule)
        {
            // Find patients assigned to doctors with wrong specialization
            var mismatchedAssignments = schedule.PatientToDoctor
                .Where(kvp => {
                    var patient = PatientsById[kvp.Key];
                    var doctor = DoctorsById[kvp.Value];
                    return doctor.Specialization != patient.RequiredSpecialization;
                })
                .OrderBy(_ => rnd.Next()) // Randomize which ones we try to fix
                .Take(3) // Limit mutations to a few at a time
                .ToList();

            foreach (var assignment in mismatchedAssignments)
            {
                int patientId = assignment.Key;
                int currentDoctorId = assignment.Value;
                var patient = PatientsById[patientId];

                // Look for available doctors with matching specialization
                if (DoctorsBySpecialization.TryGetValue(patient.RequiredSpecialization, out List<Doctor> specialists))
                {
                    var availableSpecialist = specialists
                        .Where(d => d.Id != currentDoctorId && d.Workload < d.MaxWorkload)
                        .OrderBy(d => d.Workload)
                        .FirstOrDefault();

                    if (availableSpecialist != null)
                    {
                        // Remove current assignment and reassign
                        RemovePatientAssignment(schedule, patientId);
                        AssignPatientToDoctor(schedule, patientId, availableSpecialist.Id);
                    }
                }
            }
        }

        private void MutateForWorkloadBalance(Schedule schedule)
        {
            // Identify overworked and underworked doctors
            var doctorWorkloads = schedule.DoctorToPatients
                .Select(kvp => new {
                    DoctorId = kvp.Key,
                    Doctor = DoctorsById[kvp.Key],
                    Workload = kvp.Value.Count,
                    WorkloadPercent = (double)kvp.Value.Count / DoctorsById[kvp.Key].MaxWorkload
                })
                .ToList();

            if (doctorWorkloads.Count < 2) return; // Need at least two doctors to balance

            var overworkedDocs = doctorWorkloads
                .OrderByDescending(d => d.WorkloadPercent)
                .Take(2)
                .ToList();

            var underworkedDocs = doctorWorkloads
                .OrderBy(d => d.WorkloadPercent)
                .Take(2)
                .ToList();

            // Try to balance by moving some patients
            foreach (var overworked in overworkedDocs)
            {
                foreach (var underworked in underworkedDocs)
                {
                    if (overworked.WorkloadPercent - underworked.WorkloadPercent < 0.2)
                        continue; // Skip if difference is small

                    // Find a patient from overworked doctor that underworked doctor can handle
                    var transferablePatients = schedule.DoctorToPatients[overworked.DoctorId]
                        .Where(patientId => {
                            var patient = PatientsById[patientId];
                            // Prioritize patients that match the underworked doctor's specialization
                            return (underworked.Doctor.Specialization == patient.RequiredSpecialization) &&
                                   (underworked.Doctor.ExperienceLevel >= GetRequiredExperienceLevel(patient.Urgency));
                        })
                        .OrderBy(_ => rnd.Next())
                        .Take(1)
                        .ToList();

                    foreach (var patientId in transferablePatients)
                    {
                        RemovePatientAssignment(schedule, patientId);
                        AssignPatientToDoctor(schedule, patientId, underworked.DoctorId);
                    }
                }
            }
        }

        private void MutateForContinuity(Schedule schedule)
        {
            // Improve continuity of care by reassigning patients to previous doctors
            var potentialContinuityImprovements = schedule.PatientToDoctor
                .Where(kvp =>
                    previousAssignments.TryGetValue(kvp.Key, out int prevDoc) &&
                    prevDoc != kvp.Value &&
                    DoctorsById.ContainsKey(prevDoc) &&
                    DoctorsById[prevDoc].Workload < DoctorsById[prevDoc].MaxWorkload)
                .OrderBy(_ => rnd.Next())
                .Take(2)
                .ToList();

            foreach (var assignment in potentialContinuityImprovements)
            {
                int patientId = assignment.Key;
                int prevDoctorId = previousAssignments[patientId];

                RemovePatientAssignment(schedule, patientId);
                AssignPatientToDoctor(schedule, patientId, prevDoctorId);
            }
        }

        private void MutateAddUnassignedPatients(Schedule schedule)
        {
            // Add unassigned patients to the schedule
            var assignedPatientIds = new HashSet<int>(schedule.PatientToDoctor.Keys);
            var unassignedPatients = PatientsById.Values
                .Where(p => !p.NeedsSurgery && !assignedPatientIds.Contains(p.Id))
                .OrderByDescending(p => p.GetUrgencyValue())
                .ThenBy(_ => rnd.Next())
                .Take(3)
                .ToList();

            foreach (var patient in unassignedPatients)
            {
                // Find a suitable doctor with capacity
                Doctor selectedDoctor = null;

                // First try a doctor with matching specialization
                if (DoctorsBySpecialization.TryGetValue(patient.RequiredSpecialization, out List<Doctor> specialists))
                {
                    selectedDoctor = specialists
                        .Where(d => d.Workload < d.MaxWorkload)
                        .OrderBy(d => d.Workload)
                        .FirstOrDefault();
                }

                // If no matching specialist is available, try any doctor with capacity
                if (selectedDoctor == null)
                {
                    selectedDoctor = DoctorsById.Values
                        .Where(d => d.Workload < d.MaxWorkload)
                        .OrderBy(d => d.Workload)
                        .FirstOrDefault();
                }

                if (selectedDoctor != null)
                {
                    AssignPatientToDoctor(schedule, patient.Id, selectedDoctor.Id);
                }
            }
        }
        #endregion

        #region Utility Methods
        private void AssignRemainingPatients(Schedule schedule)
        {
            // Get urgent patients that haven't been assigned yet
            var assignedPatientIds = new HashSet<int>(schedule.PatientToDoctor.Keys);

            var urgentUnassignedPatients = PatientsById.Values
                .Where(p => !p.NeedsSurgery && !assignedPatientIds.Contains(p.Id) && p.GetUrgencyValue() >= 2)
                .OrderByDescending(p => p.GetUrgencyValue())
                .ThenBy(_ => rnd.Next())
                .ToList();

            foreach (var patient in urgentUnassignedPatients)
            {
                // Try specialists first
                List<Doctor> potentialDoctors = null;

                if (DoctorsBySpecialization.TryGetValue(patient.RequiredSpecialization, out List<Doctor> specialists))
                {
                    potentialDoctors = specialists
                        .Where(d => d.Workload < d.MaxWorkload)
                        .OrderBy(d => d.Workload)
                        .ToList();
                }

                // If no specialists available, try any doctor
                if (potentialDoctors == null || !potentialDoctors.Any())
                {
                    potentialDoctors = DoctorsById.Values
                        .Where(d => d.Workload < d.MaxWorkload)
                        .OrderBy(d => d.Workload)
                        .ToList();
                }

                if (potentialDoctors.Any())
                {
                    AssignPatientToDoctor(schedule, patient.Id, potentialDoctors.First().Id);
                }
            }
        }

        private void AssignPatientToDoctor(Schedule schedule, int patientId, int doctorId)
        {
            // If the patient is already assigned to a doctor, remove them from that doctor's list
            if (schedule.PatientToDoctor.ContainsKey(patientId))
            {
                RemovePatientAssignment(schedule, patientId);
            }

            // If the doctor is not already in the schedule, add them
            if (!schedule.DoctorToPatients.ContainsKey(doctorId))
            {
                schedule.DoctorToPatients[doctorId] = new List<int>();
            }

            // Assign the patient to the doctor
            schedule.DoctorToPatients[doctorId].Add(patientId);
            schedule.PatientToDoctor[patientId] = doctorId;

            // Update the doctor's workload
            if (DoctorsById.TryGetValue(doctorId, out Doctor doctor))
            {
                doctor.Workload++;
            }
        }

        private void RemovePatientAssignment(Schedule schedule, int patientId)
        {
            if (schedule.PatientToDoctor.TryGetValue(patientId, out int currentDoctorId))
            {
                if (schedule.DoctorToPatients.ContainsKey(currentDoctorId))
                {
                    schedule.DoctorToPatients[currentDoctorId].Remove(patientId);

                    // Clean up empty lists
                    if (!schedule.DoctorToPatients[currentDoctorId].Any())
                    {
                        schedule.DoctorToPatients.Remove(currentDoctorId);
                    }

                    // Update the doctor's workload
                    if (DoctorsById.TryGetValue(currentDoctorId, out Doctor doctor))
                    {
                        doctor.Workload = Math.Max(0, doctor.Workload - 1);
                    }
                }

                schedule.PatientToDoctor.Remove(patientId);
            }
        }

        private double ScoreSchedule(Schedule schedule)
        {
            double score = 0;

            // Score for each doctor-patient assignment
            foreach (var doctorId in schedule.DoctorToPatients.Keys)
            {
                if (DoctorsById.TryGetValue(doctorId, out Doctor doctor))
                {
                    var patients = schedule.DoctorToPatients[doctorId];

                    // Base score for each patient assigned
                    score += patients.Count * patientAssignmentWeight;

                    // Workload balance score (penalty for high workload percentage)
                    double workloadFactor = (double)doctor.Workload / doctor.MaxWorkload;
                    double workloadScore = (1 - Math.Pow(workloadFactor, 2)) * workloadBalanceWeight * patients.Count;
                    score += workloadScore;

                    // Score for each patient
                    foreach (var patientId in patients)
                    {
                        if (PatientsById.TryGetValue(patientId, out Patient patient))
                        {
                            // Specialization match bonus
                            if (doctor.Specialization == patient.RequiredSpecialization)
                            {
                                score += specializationMatchWeight;
                            }

                            // Urgency handling bonus
                            score += patient.GetUrgencyValue() * urgencyWeight;

                            // Experience level appropriate for complexity
                            int requiredLevel = GetRequiredExperienceLevel(patient.Urgency);
                            if (doctor.ExperienceLevel >= requiredLevel)
                            {
                                score += experienceLevelWeight;

                                // Additional bonus if the doctor is at the perfect level (not overqualified)
                                if (doctor.ExperienceLevel == requiredLevel)
                                {
                                    score += hierarchyWeight;
                                }
                            }

                            // Continuity of care bonus
                            if (previousAssignments.TryGetValue(patientId, out int prevDoctorId) && prevDoctorId == doctorId)
                            {
                                score += continuityOfCareWeight;
                            }
                        }
                    }
                }
            }

            return score;
        }

        private Schedule CloneSchedule(Schedule original)
        {
            var clone = new Schedule
            {
                DoctorToPatients = new Dictionary<int, List<int>>(),
                PatientToDoctor = new Dictionary<int, int>()
            };

            // Deep copy of DoctorToPatients
            foreach (var pair in original.DoctorToPatients)
            {
                clone.DoctorToPatients[pair.Key] = new List<int>(pair.Value);
            }

            // Copy PatientToDoctor
            foreach (var pair in original.PatientToDoctor)
            {
                clone.PatientToDoctor[pair.Key] = pair.Value;
            }

            return clone;
        }

        private int GetRequiredExperienceLevel(string urgency)
        {
            switch (urgency.ToLower())
            {
                case "high": return 3; // High urgency requires senior doctors
                case "medium": return 2; // Medium urgency requires at least regular doctors
                default: return 1; // Low urgency can be handled by any doctor
            }
        }

        private bool TerminationConditionMet()
        {
            // Condition 1: Maximum generations reached
            if (currentGeneration >= maxGenerations)
            {
                Console.WriteLine("Termination: Maximum generations reached.");
                return true;
            }

            // Condition 2: Fitness threshold met
            if (bestFitness >= fitnessThreshold)
            {
                Console.WriteLine("Termination: Fitness threshold reached.");
                return true;
            }

            // Condition 3: Stagnation (no improvement for too long)
            if (stagnationCount >= maxStagnation)
            {
                Console.WriteLine("Termination: Stagnation detected.");
                return true;
            }

            return false;
        }

        private void PrintScheduleDetails(Schedule schedule)
        {
            int assignedPatients = schedule.PatientToDoctor.Count;
            int totalPatientsNeedingDoctor = PatientsById.Values.Count(p => !p.NeedsSurgery);
            double assignmentPercentage = (double)assignedPatients / totalPatientsNeedingDoctor * 100;

            Console.WriteLine($"Patients assigned: {assignedPatients}/{totalPatientsNeedingDoctor} ({assignmentPercentage:F1}%)");

            // Doctor workload distribution
            var doctorWorkloads = schedule.DoctorToPatients.Keys
                .Select(id => new {
                    DoctorId = id,
                    Count = schedule.DoctorToPatients[id].Count,
                    Doctor = DoctorsById[id],
                    Percentage = (double)schedule.DoctorToPatients[id].Count / DoctorsById[id].MaxWorkload * 100
                })
                .ToList();

            if (doctorWorkloads.Any())
            {
                double minWorkload = doctorWorkloads.Min(d => d.Percentage);
                double maxWorkload = doctorWorkloads.Max(d => d.Percentage);
                double avgWorkload = doctorWorkloads.Average(d => d.Percentage);
                double stdDeviation = Math.Sqrt(doctorWorkloads.Sum(d => Math.Pow(d.Percentage - avgWorkload, 2)) / doctorWorkloads.Count);

                Console.WriteLine($"Doctor workload - Min: {minWorkload:F1}%, Max: {maxWorkload:F1}%, Avg: {avgWorkload:F1}%, StdDev: {stdDeviation:F1}%");

                // Print workload by specialization
                var specializationWorkloads = doctorWorkloads
                    .GroupBy(d => d.Doctor.Specialization)
                    .Select(g => new {
                        Specialization = g.Key,
                        AvgWorkload = g.Average(d => d.Percentage),
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.AvgWorkload)
                    .ToList();

                Console.WriteLine("Workload by specialization:");
                foreach (var spec in specializationWorkloads)
                {
                    Console.WriteLine($"  {spec.Specialization}: {spec.AvgWorkload:F1}% (from {spec.Count} doctors)");
                }
            }
        }
    }
}
#endregion