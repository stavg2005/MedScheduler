using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
// Assuming your Models namespace contains Doctor, Patient, Surgeon, Schedule, Enums, etc.
using ClassLibrary1;

namespace Models// Or your appropriate namespace
{


    /// <summary>
    /// Implements the genetic algorithm for scheduling doctors to patients.
    /// </summary>
    public class DoctorScheduler
    {
        #region Data Members
        // Core collections
        private List<Schedule> Population { get; set; }
        private readonly int populationSize;
        private readonly Random rnd = new Random();

        // Resource collections - using concurrent dictionary for thread safety if needed,
        // but could be standard Dictionary if populated once at the start.
        private Dictionary<int, Doctor> DoctorsById { get; set; }
        private Dictionary<int, Patient> PatientsById { get; set; }

        // Sorted collections for potentially efficient lookup (optional)
        private Dictionary<string, List<Doctor>> DoctorsBySpecialization { get; set; }
        // Store patients grouped by UrgencyLevel Enum
        private SortedDictionary<UrgencyLevel, List<Patient>> PatientsByUrgency { get; set; }

        // Termination condition variables
        public int maxGenerations = 500;
        public int currentGeneration = 0;
        public double fitnessThreshold = 100000; // Adjust as needed based on scoring scale
        public int stagnationCount = 0;
        public int maxStagnation = 100;
        public double bestFitness = double.MinValue;
        public double previousBestFitness = double.MinValue;

        // Genetic algorithm parameters
        private readonly double crossoverRate = 0.8;
        private readonly double mutationRate = 0.1; 
        private readonly int tournamentSize = 7;
        private readonly int elitismCount; // Percentage of population to carry over

        // Weights for fitness function - ADJUST THESE BASED ON PRIORITIES
        private readonly double specializationMatchWeight = 6.0;
        private readonly double urgencyWeight = 4.5; // Weight per urgency level (High=3 * weight)
        private readonly double workloadBalanceWeight = 5.5; // Penalty reduction for balanced load
        private readonly double patientAssignmentWeight = 1.5; // Base score per assignment
        private readonly double continuityOfCareWeight = 2.0;
        private readonly double hierarchyWeight = 2.5; // Bonus for matching experience exactly
        private readonly double experienceLevelWeight = 3.0; // Bonus for meeting minimum experience
        private readonly double preferenceMatchWeight = 4.0; // NEW: Weight for doctor preference matching
        internal double unassignedPatientPenaltyMultiplier = 5.0; // Multiplier for urgencyWeight penalty
        // For tracking assignments for continuity of care (simple version)
        // Key: PatientId, Value: Last known DoctorId from input data
        private Dictionary<int, int> previousAssignments = new Dictionary<int, int>();
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the DoctorScheduler.
        /// </summary>
        /// <param name="populationSize">Number of candidate schedules in each generation.</param>
        /// <param name="doctors">List of available doctors (MUST have Preferences populated).</param>
        /// <param name="patients">List of patients needing assignment.</param>
        public DoctorScheduler(int populationSize, List<Doctor> doctors, List<Patient> patients)
        {
            if (doctors == null || !doctors.Any() || patients == null)
            {
                throw new ArgumentException("Doctors and patients lists cannot be null or empty.");
            }

            this.populationSize = populationSize;
            // Ensure elitism count is at least 1 if population is small
            this.elitismCount = Math.Max(1, (int)(populationSize * 0.1)); // e.g., 10% elitism

            // Initialize concurrent dictionaries
            DoctorsById = new Dictionary<int, Doctor>();
            PatientsById = new Dictionary<int, Patient>();

            // Initialize sorted dictionaries
            DoctorsBySpecialization = new Dictionary<string, List<Doctor>>();
            // Use UrgencyLevel Enum as key, sort descending by the enum's underlying value
            PatientsByUrgency = new SortedDictionary<UrgencyLevel, List<Patient>>(
                Comparer<UrgencyLevel>.Create((a, b) => ((int)b).CompareTo((int)a))
            );

            // Populate the dictionaries and prepare data
            PopulateDataStructures(doctors, patients);
        }
        #endregion

        #region Data Structure Setup
        /// <summary>
        /// Populates internal data structures from the input lists.
        /// Resets doctor workloads and extracts previous assignments.
        /// </summary>
        private void PopulateDataStructures(List<Doctor> doctors, List<Patient> patients)
        {
            // Populate DoctorsById and DoctorsBySpecialization
            foreach (var doctor in doctors)
            {
                if (doctor == null) continue; // Skip null entries if any

                DoctorsById[doctor.Id] = doctor;
                doctor.Workload = 0; // Reset workload for the scheduling run

                if (!string.IsNullOrEmpty(doctor.Specialization))
                {
                    if (!DoctorsBySpecialization.ContainsKey(doctor.Specialization))
                    {
                        DoctorsBySpecialization[doctor.Specialization] = new List<Doctor>();
                    }
                    DoctorsBySpecialization[doctor.Specialization].Add(doctor);
                }
                // NOTE: Assumes doctor.Preferences list is already populated before calling the constructor
            }

            // Populate PatientsById and PatientsByUrgency
            foreach (var patient in patients)
            {
                if (patient == null) continue; // Skip null entries if any

                PatientsById[patient.Id] = patient;

                // Use the UrgencyLevel Enum directly as the key
                UrgencyLevel urgency = patient.Urgency;
                if (!PatientsByUrgency.ContainsKey(urgency))
                {
                    PatientsByUrgency[urgency] = new List<Patient>();
                }
                PatientsByUrgency[urgency].Add(patient);

                // If patient has a previous doctor assignment history, store the most recent one
                // (This is a simplification for continuity check)
                if (patient.PreviousDoctors != null && patient.PreviousDoctors.Any())
                {
                    // Consider using the last ID in the list as the "most recent"
                    previousAssignments[patient.Id] = patient.PreviousDoctors.Last();
                }
            }
        }
        #endregion

        #region Main Algorithm
        /// <summary>
        /// Runs the genetic algorithm to find an optimal schedule.
        /// </summary>
        /// <returns>The best schedule found.</returns>
        public Schedule Solve()
        {
            // Initialize the population with random (or semi-random) solutions
            Population = GeneratePopulation(populationSize);
            CalculateFitnessForAll(Population); // Calculate initial fitness

            Console.WriteLine("Initial population generated.");
            bestFitness = Population.Any() ? Population.Max(s => s.FitnessScore) : double.MinValue; // Handle empty population
            Console.WriteLine($"Best initial fitness: {bestFitness:F2}");

            // Main evolutionary loop
            while (!TerminationConditionMet())
            {
                currentGeneration++;

                List<Schedule> newPopulation = new List<Schedule>(populationSize);

                // 1. Elitism: Keep the best individuals
                var elite = Population.OrderByDescending(s => s.FitnessScore).Take(elitismCount);
                newPopulation.AddRange(elite.Select(CloneSchedule)); // Add clones of elite

                // 2. Selection, Crossover, Mutation to fill the rest
                while (newPopulation.Count < populationSize)
                {
                    Schedule parent1 = TournamentSelection();
                    Schedule parent2 = TournamentSelection();

                    // Ensure parents are not null (can happen if population becomes empty unexpectedly)
                    if (parent1 == null || parent2 == null) continue;


                    List<Schedule> offspring;
                    if (rnd.NextDouble() < crossoverRate)
                    {
                        offspring = CrossoverPMX(parent1, parent2);
                    }
                    else
                    {
                        // No crossover, just clone parents
                        offspring = new List<Schedule> { CloneSchedule(parent1), CloneSchedule(parent2) };
                    }

                    // Apply mutation
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

                // Replace the old population and calculate fitness
                Population = newPopulation;
                CalculateFitnessForAll(Population);

                // Track the best fitness and stagnation
                double currentBestFitness = Population.Any() ? Population.Max(s => s.FitnessScore) : double.MinValue;
                UpdateStagnation(currentBestFitness);
            }

            Console.WriteLine($"Genetic algorithm terminated after {currentGeneration} generations");
            var finalSchedule = GetLeadingSchedule();
            Console.WriteLine($"Final best fitness: {finalSchedule?.FitnessScore ?? double.MinValue:F2}");

            // Optional: Update persistent previous assignments if needed for future runs
            // UpdatePreviousAssignments(finalSchedule);
           
            return finalSchedule;
        }
        #endregion

        #region Population Generation
        /// <summary>
        /// Generates the initial population of candidate schedules.
        /// </summary>
        private List<Schedule> GeneratePopulation(int size)
        {
            List<Schedule> population = new List<Schedule>(size);
            // Use ConcurrentBag for thread-safe adding in parallel loop
            ConcurrentBag<Schedule> populationBag = new ConcurrentBag<Schedule>();

            Parallel.For(0, size, i =>
            {
                var schedule = GenerateInitialSchedule(); // Use a potentially smarter initial generation
                populationBag.Add(schedule);
            });

            return populationBag.ToList();
        }

        /// <summary>
        /// Generates a single initial schedule, attempting some reasonable assignments.
        /// </summary>
        private Schedule GenerateInitialSchedule()
        {
            var schedule = new Schedule();
            // Create a temporary dictionary to track workloads for this schedule generation
            var tempDoctorWorkloads = DoctorsById.ToDictionary(kvp => kvp.Key, kvp => 0);

            // Prioritize assigning patients by urgency (High -> Medium -> Low)
            // Use the PatientsByUrgency dictionary which is already sorted descending
            foreach (var urgencyGroup in PatientsByUrgency) // Iterates High -> Medium -> Low
            {
                // Shuffle patients within the same urgency level for randomness
                var patientsInUrgency = urgencyGroup.Value.OrderBy(_ => rnd.Next()).ToList();

                foreach (var patient in patientsInUrgency)
                {
                    if (patient.NeedsSurgery) continue; // Skip patients needing surgery for this scheduler

                    Doctor assignedDoctor = FindBestInitialDoctorForPatient(patient, tempDoctorWorkloads);

                    if (assignedDoctor != null)
                    {
                        AssignPatientToDoctor(schedule, patient.Id, assignedDoctor.Id);
                        tempDoctorWorkloads[assignedDoctor.Id]++; // Increment temp workload
                    }
                    // else: patient remains unassigned in this initial schedule
                }
            }
            return schedule;
        }

        /// <summary>
        /// Helper to find a suitable doctor for initial assignment, prioritizing criteria.
        /// </summary>
        private Doctor FindBestInitialDoctorForPatient(Patient patient, Dictionary<int, int> currentWorkloads)
        {
            // 1. Try previous doctor if available and suitable
            if (previousAssignments.TryGetValue(patient.Id, out int prevDoctorId) &&
                DoctorsById.TryGetValue(prevDoctorId, out Doctor prevDoctor) &&
                currentWorkloads[prevDoctorId] < prevDoctor.MaxWorkload &&
                prevDoctor.IsSuitableFor(patient)) // Use Doctor's suitability check
            {
                return prevDoctor;
            }

            // 2. Try specialists with capacity
            if (DoctorsBySpecialization.TryGetValue(patient.RequiredSpecialization, out var specialists))
            {
                var suitableSpecialist = specialists
                    .Where(d => currentWorkloads.ContainsKey(d.Id) && // Ensure doctor is in workload dict
                                currentWorkloads[d.Id] < d.MaxWorkload &&
                                d.IsSuitableFor(patient))
                    .OrderBy(d => currentWorkloads[d.Id]) // Prefer less busy
                    .FirstOrDefault();
                if (suitableSpecialist != null) return suitableSpecialist;
            }

            // 3. Try any suitable doctor with capacity
            var anySuitableDoctor = DoctorsById.Values
                .Where(d => currentWorkloads.ContainsKey(d.Id) && // Ensure doctor is in workload dict
                            currentWorkloads[d.Id] < d.MaxWorkload &&
                            d.IsSuitableFor(patient))
                .OrderBy(d => currentWorkloads[d.Id]) // Prefer less busy
                .FirstOrDefault();

            return anySuitableDoctor; // Can be null if no suitable doctor found
        }
        #endregion

        #region Selection Methods
        /// <summary>
        /// Selects a schedule using tournament selection.
        /// </summary>
        private Schedule TournamentSelection()
        {
            if (!Population.Any()) return null; // Handle empty population case

            Schedule bestInTournament = null;
            double bestFitnessInTournament = double.MinValue;

            for (int i = 0; i < tournamentSize; i++)
            {
                Schedule candidate = Population[rnd.Next(Population.Count)];
                if (candidate.FitnessScore > bestFitnessInTournament)
                {
                    bestFitnessInTournament = candidate.FitnessScore;
                    bestInTournament = candidate;
                }
            }
            // Return the best found in the tournament, or a random one if all somehow had MinValue fitness
            return bestInTournament ?? Population[rnd.Next(Population.Count)];
        }

        /// <summary>
        /// Gets the schedule with the highest fitness score from the current population.
        /// </summary>
        private Schedule GetLeadingSchedule()
        {
            if (!Population.Any()) return null;
            return Population.OrderByDescending(s => s.FitnessScore).First();
        }
        #endregion

        #region Crossover Implementation
        /// <summary>
        /// Performs crossover between two parent schedules to create two offspring.
        /// Uses a single point crossover on the list of patients.
        /// </summary>
        private List<Schedule> Crossover(Schedule parent1, Schedule parent2)
        {
            var offspring1 = new Schedule();
            var offspring2 = new Schedule();
            // Recalculate workloads based on parents for capacity checks during crossover
            var offspring1Workloads = RecalculateWorkloads(parent1); // Start with parent1's load
            var offspring2Workloads = RecalculateWorkloads(parent2); // Start with parent2's load


            // Consider all patients assigned in either parent
            var patientsToProcess = parent1.PatientToDoctor.Keys
                                     .Union(parent2.PatientToDoctor.Keys)
                                     .Distinct()
                                     .ToList();

            // Simple single crossover point
            int crossoverPoint = patientsToProcess.Any() ? rnd.Next(patientsToProcess.Count) : 0;

            for (int i = 0; i < patientsToProcess.Count; i++)
            {
                int patientId = patientsToProcess[i];

                // Determine which parent contributes the gene (assignment)
                Schedule source1 = (i < crossoverPoint) ? parent1 : parent2;
                Schedule source2 = (i < crossoverPoint) ? parent2 : parent1;

                // Try assigning for offspring 1
                TryAssignFromSource(source1, offspring1, offspring1Workloads, patientId);
                // Try assigning for offspring 2
                TryAssignFromSource(source2, offspring2, offspring2Workloads, patientId);
            }

            // Optional: Try assigning any remaining unassigned patients
            // Note: Workloads used here might not be fully accurate after crossover swaps
            AssignRemainingPatients(offspring1, RecalculateWorkloads(offspring1));
            AssignRemainingPatients(offspring2, RecalculateWorkloads(offspring2));

            return new List<Schedule> { offspring1, offspring2 };
        }

        #region Crossover Implementation - PMX Adaptation

        /// <summary>
        /// Performs Partially Mapped Crossover (PMX), adapted for the doctor-patient assignment problem.
        /// Focuses on swapping segments and resolving conflicts based on feasibility (workload, suitability).
        /// </summary>
        /// <param name="parent1">The first parent schedule.</param>
        /// <param name="parent2">The second parent schedule.</param>
        /// <returns>A list containing two new offspring schedules.</returns>
        private List<Schedule> CrossoverPMX(Schedule parent1, Schedule parent2)
        {
            // --- Step 1: Initialize Offspring and Workload Tracking ---
            var offspring1 = new Schedule();
            var offspring2 = new Schedule();

            // Track workloads dynamically for each offspring as it's built
            // Initialize with all doctors having 0 workload for this new schedule
            var offspring1Workloads = DoctorsById.Keys.ToDictionary(id => id, id => 0);
            var offspring2Workloads = DoctorsById.Keys.ToDictionary(id => id, id => 0);

            // --- Step 2: Prepare Patient Sequence ---
            // Get a consistent order of patients to treat as the "chromosome"
            // Filter out patients needing surgery as they are handled separately
            var patientSequence = PatientsById.Values
                                             .Where(p => !p.NeedsSurgery)
                                             .Select(p => p.Id) // Work with IDs
                                             .ToList();
            int n = patientSequence.Count;

            // If there are no patients to schedule, return empty schedules
            if (n == 0)
            {
                Console.WriteLine("Warning: CrossoverPMX called with no patients to schedule.");
                return new List<Schedule> { offspring1, offspring2 };
            }

            // --- Step 3: Select Crossover Points ---
            int point1 = rnd.Next(n);
            int point2 = rnd.Next(n);
            // Ensure points are distinct if possible (only matters if n > 1)
            if (n > 1 && point1 == point2)
            {
                point2 = (point1 + 1) % n;
            }
            int start = Math.Min(point1, point2);
            int end = Math.Max(point1, point2); // Segment includes indices [start, end)

            // --- Step 4: Copy Segment & Build Initial Mapping (Optional) ---
            // This dictionary maps DocInP1 -> DocInP2 for the segment, useful for conflict resolution
            var mappingP1toP2 = new Dictionary<int, int>();
            // You might also need the reverse mapping
            var mappingP2toP1 = new Dictionary<int, int>();

            // Process patients WITHIN the segment
            for (int i = start; i < end; i++)
            {
                int patientId = patientSequence[i];
                if (!PatientsById.TryGetValue(patientId, out Patient patient)) continue; // Safety check

                int? assignedDoc1 = null; // Doctor assigned in offspring1 for this patient
                int? assignedDoc2 = null; // Doctor assigned in offspring2 for this patient

                // Try assigning P2's assignment to Offspring 1
                if (parent2.PatientToDoctor.TryGetValue(patientId, out int docIdP2) &&
                    DoctorsById.TryGetValue(docIdP2, out Doctor docP2) &&
                    offspring1Workloads[docIdP2] < docP2.MaxWorkload && // Check capacity in OFFSPRING 1
                    docP2.IsSuitableFor(patient))
                {
                    AssignPatientToDoctor(offspring1, patientId, docIdP2);
                    offspring1Workloads[docIdP2]++;
                    assignedDoc1 = docIdP2; // Record the doctor assigned in offspring 1
                }

                // Try assigning P1's assignment to Offspring 2
                if (parent1.PatientToDoctor.TryGetValue(patientId, out int docIdP1) &&
                    DoctorsById.TryGetValue(docIdP1, out Doctor docP1) &&
                    offspring2Workloads[docIdP1] < docP1.MaxWorkload && // Check capacity in OFFSPRING 2
                    docP1.IsSuitableFor(patient))
                {
                    AssignPatientToDoctor(offspring2, patientId, docIdP1);
                    offspring2Workloads[docIdP1]++;
                    assignedDoc2 = docIdP1; // Record the doctor assigned in offspring 2
                }

                // If both assignments were successful, create the mapping for this position
                if (assignedDoc1.HasValue && assignedDoc2.HasValue)
                {
                    int docInO1 = assignedDoc1.Value; // Doctor assigned in offspring 1 (from P2)
                    int docInO2 = assignedDoc2.Value; // Doctor assigned in offspring 2 (from P1)

                    // Use ContainsKey check before Add for older framework compatibility
                    if (!mappingP1toP2.ContainsKey(docInO2)) // Map: DocInO2 (from P1) -> DocInO1 (from P2)
                    {
                        mappingP1toP2.Add(docInO2, docInO1);
                    }
                    if (!mappingP2toP1.ContainsKey(docInO1)) // Map: DocInO1 (from P2) -> DocInO2 (from P1)
                    {
                        mappingP2toP1.Add(docInO1, docInO2);
                    }
                }
            }

            // --- Step 5: Fill Outside Segment (Legalization) ---
            for (int i = 0; i < n; i++)
            {
                // Skip patients already processed within the segment
                if (i >= start && i < end) continue;

                int currentPatientId = patientSequence[i];
                if (!PatientsById.TryGetValue(currentPatientId, out Patient currentPatient)) continue; // Safety

                // --- Process for Offspring 1 (uses Parent 1 mainly) ---
                if (!offspring1.PatientToDoctor.ContainsKey(currentPatientId)) // If not assigned during segment copy
                {
                    // Pass the P1->P2 mapping relevant for Offspring 1
                    TryAssignPatientOutsideSegment(parent1, offspring1, offspring1Workloads, currentPatientId, currentPatient, mappingP1toP2);
                }

                // --- Process for Offspring 2 (uses Parent 2 mainly) ---
                if (!offspring2.PatientToDoctor.ContainsKey(currentPatientId)) // If not assigned during segment copy
                {
                    // Pass the P2->P1 mapping relevant for Offspring 2
                    TryAssignPatientOutsideSegment(parent2, offspring2, offspring2Workloads, currentPatientId, currentPatient, mappingP2toP1);
                }
            }

            // --- Step 6: Handle Remaining Unassigned (Optional but Recommended) ---
            // Use RecalculateWorkloads to ensure accurate counts before this final pass
            AssignRemainingPatients(offspring1, RecalculateWorkloads(offspring1));
            AssignRemainingPatients(offspring2, RecalculateWorkloads(offspring2));

            // --- Step 7: Return Offspring ---
            return new List<Schedule> { offspring1, offspring2 };
        }

        /// <summary>
        /// Helper method for PMX Crossover Step 5 (Legalization).
        /// Tries to assign a patient outside the initial segment, prioritizing direct parent assignment,
        /// then mapped assignment (if available and feasible), then a fallback search.
        /// </summary>
        /// <param name="sourceParent">The parent providing the primary gene for this position.</param>
        /// <param name="targetOffspring">The offspring schedule being built.</param>
        /// <param name="offspringWorkloads">The current workload tracking for the target offspring.</param>
        /// <param name="patientId">The ID of the patient to assign.</param>
        /// <param name="patient">The Patient object.</param>
        /// <param name="segmentMapping">Mapping dictionary (SourceParentDoc -> OtherParentDoc) derived from the segment swap.</param>
        private void TryAssignPatientOutsideSegment(
            Schedule sourceParent,
            Schedule targetOffspring,
            Dictionary<int, int> offspringWorkloads,
            int patientId,
            Patient patient,
            Dictionary<int, int> segmentMapping)
        {
            bool assigned = false;

            // Attempt 1: Direct assignment from source parent
            if (sourceParent.PatientToDoctor.TryGetValue(patientId, out int sourceDoctorId) &&
                DoctorsById.TryGetValue(sourceDoctorId, out Doctor sourceDoctor) &&
                offspringWorkloads[sourceDoctorId] < sourceDoctor.MaxWorkload && // Check capacity in OFFSPRING
                sourceDoctor.IsSuitableFor(patient))                               // Check suitability
            {
                AssignPatientToDoctor(targetOffspring, patientId, sourceDoctorId);
                offspringWorkloads[sourceDoctorId]++;
                assigned = true;
                // Console.WriteLine($"PMX Assign [{patientId}]: Direct from Parent {sourceDoctorId}"); // Debug
            }

            // Attempt 2: Try mapped doctor if direct assignment failed or wasn't possible, and mapping exists
            if (!assigned && sourceParent.PatientToDoctor.TryGetValue(patientId, out sourceDoctorId) /* only try if source had *some* assignment */)
            {
                if (segmentMapping.TryGetValue(sourceDoctorId, out int mappedDoctorId) &&
                    DoctorsById.TryGetValue(mappedDoctorId, out Doctor mappedDoctor) &&
                    offspringWorkloads[mappedDoctorId] < mappedDoctor.MaxWorkload && // Check capacity of MAPPED doctor
                    mappedDoctor.IsSuitableFor(patient))                             // Check suitability of MAPPED doctor
                {
                    AssignPatientToDoctor(targetOffspring, patientId, mappedDoctorId);
                    offspringWorkloads[mappedDoctorId]++;
                    assigned = true;
                    // Console.WriteLine($"PMX Assign [{patientId}]: Mapped from {sourceDoctorId} -> {mappedDoctorId}"); // Debug
                }
            }

            //Attempt 3: Fallback if still not assigned (Optional, could rely on AssignRemainingPatients later)
            // You might choose *not* to include this fallback directly within crossover
            // to keep the operator simpler and rely on the final pass or mutation.
            
            if (!assigned)
            {
                // Find *any* suitable doctor with capacity using the current offspring workloads
                Doctor fallbackDoctor = FindBestInitialDoctorForPatient(patient, offspringWorkloads); // Re-use existing logic
                if (fallbackDoctor != null)
                {
                    AssignPatientToDoctor(targetOffspring, patientId, fallbackDoctor.Id);
                    offspringWorkloads[fallbackDoctor.Id]++;
                    assigned = true;
                      Console.WriteLine($"PMX Assign [{patientId}]: Fallback {fallbackDoctor.Id}"); // Debug
                }
            }
            

            // If not assigned after all attempts, the patient remains unassigned in this offspring for now.
            // The optional Step 6 (AssignRemainingPatients) might assign them later.
        }

        #endregion // Crossover Implementation - PMX Adaptation

        /// <summary>
        /// Helper for crossover: Tries to assign a patient to an offspring from a source parent.
        /// Updates the temporary workload dictionary for the offspring.
        /// </summary>
        private void TryAssignFromSource(Schedule source, Schedule offspring, Dictionary<int, int> offspringWorkloads, int patientId)
        {
            // Ensure patient isn't already assigned in offspring
            if (offspring.PatientToDoctor.ContainsKey(patientId)) return;

            if (source.PatientToDoctor.TryGetValue(patientId, out int doctorId) &&
                DoctorsById.TryGetValue(doctorId, out Doctor doctor) &&
                offspringWorkloads.TryGetValue(doctorId, out int currentLoad) && // Check if doctor exists in workload dict
                currentLoad < doctor.MaxWorkload)
            {
                AssignPatientToDoctor(offspring, patientId, doctorId);
                offspringWorkloads[doctorId]++; // Increment workload count
            }
            // If assignment fails (e.g., doctor full), patient remains unassigned for now
        }
        #endregion

        #region Mutation Implementation
        /// <summary>
        /// Applies a random mutation strategy to a schedule.
        /// </summary>
        private void Mutate(Schedule schedule)
        {
            var currentWorkloads = RecalculateWorkloads(schedule);
            // Increase chances of mutations aimed at fixing assignments
            int mutationType = rnd.Next(100); // Use 0-99 range

            if (mutationType < 25) // 25% chance: Add Unassigned (Increased chance)
                MutateAddUnassignedPatients(schedule, currentWorkloads);
            else if (mutationType < 45) // 20% chance: Optimize Preferences
                MutateOptimizePreferences(schedule, currentWorkloads);
            else if (mutationType < 65) // 20% chance: Improve Continuity
                MutateForContinuity(schedule, currentWorkloads);
            else if (mutationType < 85) // 20% chance: Balance Workload
                MutateForWorkloadBalance(schedule, currentWorkloads);
            else // 15% chance: Simple Reassign
                MutateReassignPatient(schedule, currentWorkloads);

            // Could add swap mutation here as well
        }

        // --- Other Mutation Methods (MutateReassignPatient, MutateForWorkloadBalance, etc. - keep as before) ---

        /// <summary>
        /// Mutation: Randomly reassigns a single patient to a different suitable doctor.
        /// </summary>
        private void MutateReassignPatient(Schedule schedule, Dictionary<int, int> currentWorkloads)
        {
            if (!schedule.PatientToDoctor.Any()) return;

            // Select a random assigned patient
            var patientAssignmentList = schedule.PatientToDoctor.ToList(); // Avoid modifying while iterating indirectly
            var patientAssignment = patientAssignmentList[rnd.Next(patientAssignmentList.Count)];
            int patientId = patientAssignment.Key;
            int currentDoctorId = patientAssignment.Value;

            if (!PatientsById.TryGetValue(patientId, out var patient)) return; // Safety check

            // Find other suitable doctors with capacity
            var potentialDoctors = DoctorsById.Values
                .Where(d => d.Id != currentDoctorId &&
                            currentWorkloads.ContainsKey(d.Id) && // Ensure doctor is trackable
                            currentWorkloads[d.Id] < d.MaxWorkload &&
                            d.IsSuitableFor(patient))
                .ToList();

            if (potentialDoctors.Any())
            {
                // Choose a random new doctor from the suitable ones
                var newDoctor = potentialDoctors[rnd.Next(potentialDoctors.Count)];
                // Remove old assignment and add new one
                RemovePatientAssignment(schedule, patientId);
                AssignPatientToDoctor(schedule, patientId, newDoctor.Id);
                // Note: Workloads are conceptually updated by Assign/Remove, no need to update currentWorkloads dict here
            }
        }

        /// <summary>
        /// Mutation: Tries to move a patient from an overworked doctor to an underworked one.
        /// </summary>
        private void MutateForWorkloadBalance(Schedule schedule, Dictionary<int, int> currentWorkloads)
        {
            var doctors = DoctorsById.Values.ToList();
            if (doctors.Count < 2) return;

            // Find most and least loaded doctors (considering MaxWorkload)
            var sortedByLoad = doctors
                .Where(d => currentWorkloads.ContainsKey(d.Id)) // Only consider doctors with assignments or potential
                .OrderBy(d => d.MaxWorkload == 0 ? double.MaxValue : (double)currentWorkloads[d.Id] / d.MaxWorkload) // Handle MaxWorkload=0
                .ToList();

            if (sortedByLoad.Count < 2) return; // Need at least two doctors with workloads

            var leastLoaded = sortedByLoad.First();
            var mostLoaded = sortedByLoad.Last();

            // Only attempt if there's a significant difference and space available
            if (mostLoaded.Id == leastLoaded.Id || !schedule.DoctorToPatients.ContainsKey(mostLoaded.Id)) return;

            double mostLoadPercent = mostLoaded.MaxWorkload == 0 ? 1.0 : (double)currentWorkloads[mostLoaded.Id] / mostLoaded.MaxWorkload;
            double leastLoadPercent = leastLoaded.MaxWorkload == 0 ? 1.0 : (double)currentWorkloads[leastLoaded.Id] / leastLoaded.MaxWorkload;
            double loadDiff = mostLoadPercent - leastLoadPercent;


            if (loadDiff > 0.2 && currentWorkloads[leastLoaded.Id] < leastLoaded.MaxWorkload) // Threshold difference
            {
                // Find a patient assigned to the most loaded doctor that the least loaded can take
                var transferablePatientId = schedule.DoctorToPatients[mostLoaded.Id]
                    .Select(pid => PatientsById.TryGetValue(pid, out var p) ? p : null) // Get patient object safely
                    .Where(p => p != null && leastLoaded.IsSuitableFor(p)) // Check suitability
                    .OrderBy(_ => rnd.Next()) // Random choice among suitable patients
                    .Select(p => p.Id)
                    .FirstOrDefault(); // Get the ID

                if (transferablePatientId != 0) // Check if a patient was found
                {
                    RemovePatientAssignment(schedule, transferablePatientId);
                    AssignPatientToDoctor(schedule, transferablePatientId, leastLoaded.Id);
                }
            }
        }

        /// <summary>
        /// Mutation: Tries to assign a patient back to their previous doctor if possible.
        /// *** CORRECTED VERSION ***
        /// </summary>
        private void MutateForContinuity(Schedule schedule, Dictionary<int, int> currentWorkloads)
        {
            var candidates = schedule.PatientToDoctor
                .Select(kvp => new { PatientId = kvp.Key, CurrentDoctorId = kvp.Value }) // Select relevant data
                .Where(item =>
                    // Check if a previous assignment exists for this patient
                    previousAssignments.TryGetValue(item.PatientId, out int prevDocId) &&
                    // Ensure the previous doctor is different from the current one
                    prevDocId != item.CurrentDoctorId &&
                    // Ensure the previous doctor exists in our main list and workload tracking
                    DoctorsById.TryGetValue(prevDocId, out Doctor prevDoctor) &&
                    currentWorkloads.ContainsKey(prevDocId) &&
                    // Ensure the previous doctor has capacity
                    currentWorkloads[prevDocId] < prevDoctor.MaxWorkload &&
                    // Ensure the previous doctor is suitable for the patient
                    prevDoctor.IsSuitableFor(PatientsById[item.PatientId]))
                .OrderBy(_ => rnd.Next())
                .Take(1) // Try one at random
                .ToList(); // Execute the query

            // The 'candidates' list now contains items where all conditions were met

            foreach (var candidate in candidates)
            {
                int patientId = candidate.PatientId;
                // Safely get the previous doctor ID again (it's guaranteed to exist here)
                if (previousAssignments.TryGetValue(patientId, out int doctorToAssign))
                {
                    Console.WriteLine($"MutateContinuity: Moving P{patientId} back to previous Dr{doctorToAssign}");
                    RemovePatientAssignment(schedule, patientId);
                    AssignPatientToDoctor(schedule, patientId, doctorToAssign);
                }
            }
        }


        /// <summary>
        /// Mutation: Tries to assign currently unassigned patients.
        /// </summary>
       

        private void MutateAddUnassignedPatients(Schedule schedule, Dictionary<int, int> currentWorkloads)
        {
            var assignedPatientIds = new HashSet<int>(schedule.PatientToDoctor.Keys);
            var unassigned = PatientsById.Values
                .Where(p => !assignedPatientIds.Contains(p.Id)) // Already filtered for non-surgery
                .OrderByDescending(p => (int)p.Urgency)
                .ThenBy(_ => rnd.Next())
                .Take(5) // *** INCREASED: Try to assign more unassigned patients ***
                .ToList();

            int assignedCount = 0;
            foreach (var patient in unassigned)
            {
                var suitableDoctor = FindBestInitialDoctorForPatient(patient, currentWorkloads);
                if (suitableDoctor != null)
                {
                    // Ensure workload dict reflects reality before checking capacity again
                    int currentLoad = currentWorkloads.ContainsKey(suitableDoctor.Id) ? currentWorkloads[suitableDoctor.Id] : 0;
                    if (currentLoad < suitableDoctor.MaxWorkload)
                    {
                        AssignPatientToDoctor(schedule, patient.Id, suitableDoctor.Id);
                        currentWorkloads[suitableDoctor.Id]++; // Update local workload count
                        assignedCount++;
                    }
                }
            }
            if (assignedCount > 0) Console.WriteLine($"MutateAddUnassigned: Added {assignedCount} patients.");
        }

        /// <summary>
        /// Mutation: Tries to improve preference matching by swapping patients.
        /// </summary>
        private void MutateOptimizePreferences(Schedule schedule, Dictionary<int, int> currentWorkloads)
        {
            if (schedule.PatientToDoctor.Count < 2) return; // Need at least two assignments to swap

            // Get assignments as a list to pick random indices
            var assignments = schedule.PatientToDoctor.ToList();

            // Select two distinct random assignments
            int index1 = rnd.Next(assignments.Count);
            int index2 = rnd.Next(assignments.Count);
            if (index1 == index2) index2 = (index1 + 1) % assignments.Count; // Ensure distinct if possible

            var assignment1 = assignments[index1];
            var assignment2 = assignments[index2];


            int patient1Id = assignment1.Key;
            int doctor1Id = assignment1.Value;
            int patient2Id = assignment2.Key;
            int doctor2Id = assignment2.Value;

            // Ensure we have valid data
            if (!PatientsById.TryGetValue(patient1Id, out var patient1) ||
                !DoctorsById.TryGetValue(doctor1Id, out var doctor1) ||
                !PatientsById.TryGetValue(patient2Id, out var patient2) ||
                !DoctorsById.TryGetValue(doctor2Id, out var doctor2))
            {
                return; // Skip if data is missing
            }


            // Calculate current preference scores
            double currentScore1 = CalculatePreferenceScoreForPair(doctor1, patient1);
            double currentScore2 = CalculatePreferenceScoreForPair(doctor2, patient2);

            // Calculate potential preference scores if swapped
            double potentialScore1 = CalculatePreferenceScoreForPair(doctor1, patient2);
            double potentialScore2 = CalculatePreferenceScoreForPair(doctor2, patient1);

            // Check if swapping improves the *total* preference score AND if doctors are suitable for the swapped patient
            if ((potentialScore1 + potentialScore2 > currentScore1 + currentScore2) &&
                doctor1.IsSuitableFor(patient2) && doctor2.IsSuitableFor(patient1))
            {
                Console.WriteLine($"MutatePreference: Swapping P{patient1Id}(Dr{doctor1Id}) and P{patient2Id}(Dr{doctor2Id})");
                // Perform the swap
                RemovePatientAssignment(schedule, patient1Id);
                RemovePatientAssignment(schedule, patient2Id);
                AssignPatientToDoctor(schedule, patient1Id, doctor2Id);
                AssignPatientToDoctor(schedule, patient2Id, doctor1Id);
            }
        }
        #endregion

        #region Utility Methods

        /// <summary>
        /// Recalculates the current workload for each doctor based on a given schedule.
        /// Initializes workload for doctors even if they have no assignments in this schedule.
        /// </summary>
        private Dictionary<int, int> RecalculateWorkloads(Schedule schedule)
        {
            // Start with all doctors having 0 workload
            var workloads = DoctorsById.Keys.ToDictionary(id => id, id => 0);
            // Count assignments from the schedule
            foreach (var patientId in schedule.PatientToDoctor.Keys)
            {
                int doctorId = schedule.PatientToDoctor[patientId];
                if (workloads.ContainsKey(doctorId))
                {
                    workloads[doctorId]++;
                }
                else
                {
                    // This case should ideally not happen if Patients are only assigned to known DoctorsById
                    // Log warning or handle appropriately
                }
            }
            return workloads;
        }


        /// <summary>
        /// Assigns currently unassigned patients if possible. Used after crossover.
        /// </summary>
        private void AssignRemainingPatients(Schedule schedule, Dictionary<int, int> currentWorkloads)
        {
            var assignedPatientIds = new HashSet<int>(schedule.PatientToDoctor.Keys);
            var unassigned = PatientsById.Values
                .Where(p => !p.NeedsSurgery && !assignedPatientIds.Contains(p.Id))
                .OrderByDescending(p => (int)p.Urgency)
                .ToList();

            foreach (var patient in unassigned)
            {
                var suitableDoctor = FindBestInitialDoctorForPatient(patient, currentWorkloads);
                if (suitableDoctor != null)
                {
                    // Double-check capacity before assigning
                    if (currentWorkloads[suitableDoctor.Id] < suitableDoctor.MaxWorkload)
                    {
                        AssignPatientToDoctor(schedule, patient.Id, suitableDoctor.Id);
                        currentWorkloads[suitableDoctor.Id]++;
                    }
                }
            }
        }

        /// <summary>
        /// Assigns a patient to a doctor within a schedule, updating mappings.
        /// NOTE: Does NOT update the shared Doctor.Workload property directly. Workloads should be recalculated per schedule.
        /// </summary>
        private void AssignPatientToDoctor(Schedule schedule, int patientId, int doctorId)
        {
            // Ensure doctor list exists in the schedule's dictionary
            if (!schedule.DoctorToPatients.ContainsKey(doctorId))
            {
                schedule.DoctorToPatients[doctorId] = new List<int>();
            }

            // Add assignment (handle potential duplicates if logic allows)
            if (!schedule.DoctorToPatients[doctorId].Contains(patientId))
            {
                schedule.DoctorToPatients[doctorId].Add(patientId);
            }
            schedule.PatientToDoctor[patientId] = doctorId;
        }

        /// <summary>
        /// Removes a patient's assignment from a schedule, updating mappings.
        /// NOTE: Does NOT update the shared Doctor.Workload property directly.
        /// </summary>
        private void RemovePatientAssignment(Schedule schedule, int patientId)
        {
            if (schedule.PatientToDoctor.TryGetValue(patientId, out int currentDoctorId))
            {
                if (schedule.DoctorToPatients.TryGetValue(currentDoctorId, out var patientList))
                {
                    patientList.Remove(patientId);
                    if (!patientList.Any())
                    {
                        schedule.DoctorToPatients.Remove(currentDoctorId); // Clean up empty list
                    }
                }
                schedule.PatientToDoctor.Remove(patientId);
            }
        }

        /// <summary>
        /// Calculates the fitness score for a given schedule.
        /// Higher scores are better.
        /// </summary>
        private double ScoreSchedule(Schedule schedule)
        {
            if (schedule.FitnessScore >= 0) return schedule.FitnessScore; // Use cache

            double score = 0;
            var currentWorkloads = RecalculateWorkloads(schedule);

            // Score assignments
            foreach (var assignment in schedule.PatientToDoctor)
            { /* ... existing scoring logic ... */
                int patientId = assignment.Key; int doctorId = assignment.Value;
                if (!PatientsById.TryGetValue(patientId, out Patient patient) || !DoctorsById.TryGetValue(doctorId, out Doctor doctor)) continue;
                score += patientAssignmentWeight;
                if (doctor.Specialization == patient.RequiredSpecialization) score += specializationMatchWeight; else score -= specializationMatchWeight; // Penalty added
                score += (int)patient.Urgency * urgencyWeight;
                ExperienceLevel requiredLevel = GetRequiredExperienceLevel(patient.Urgency);
                if (doctor.ExperienceLevel >= requiredLevel) { score += experienceLevelWeight; if (doctor.ExperienceLevel == requiredLevel) score += hierarchyWeight; } else { score -= experienceLevelWeight * 2; } // Penalty added
                if (previousAssignments.TryGetValue(patientId, out int prevDoctorId) && prevDoctorId == doctorId) score += continuityOfCareWeight;
                score += CalculatePreferenceScoreForPair(doctor, patient) * preferenceMatchWeight;
            }

            // Score workload balance
            double totalWorkloadFactorSquaredSum = 0; int doctorsConsideredForWorkload = 0;
            foreach (var docId in DoctorsById.Keys) { if (DoctorsById.TryGetValue(docId, out Doctor doctor) && doctor.MaxWorkload > 0) { doctorsConsideredForWorkload++; int load = currentWorkloads.TryGetValue(docId, out int currentLoad) ? currentLoad : 0; double workloadFactor = (double)load / doctor.MaxWorkload; totalWorkloadFactorSquaredSum += Math.Pow(workloadFactor, 2); } }
            double avgWorkloadFactorSquared = doctorsConsideredForWorkload > 0 ? totalWorkloadFactorSquaredSum / doctorsConsideredForWorkload : 0;
            score += (1.0 - avgWorkloadFactorSquared) * workloadBalanceWeight * doctorsConsideredForWorkload;

            // *** INCREASED PENALTY for unassigned patients ***
            double unassignedPenalty = 0;
            var assignedIds = new HashSet<int>(schedule.PatientToDoctor.Keys);
            foreach (var patient in PatientsById.Values) // Already filtered for non-surgery
            {
                if (!assignedIds.Contains(patient.Id))
                {
                    // Base penalty + urgency scaling + higher multiplier
                    unassignedPenalty += ((int)patient.Urgency * urgencyWeight * unassignedPatientPenaltyMultiplier);
                    // Optionally add a large fixed penalty per unassigned patient too
                    // unassignedPenalty += 50; // Example fixed penalty
                }
            }
            score -= unassignedPenalty; // Subtract the total penalty

            schedule.FitnessScore = score;
            return score;
        }

        /// <summary>
        /// Calculates the preference match score (0 to 1) for a single doctor-patient pair.
        /// </summary>
        private double CalculatePreferenceScoreForPair(Doctor doctor, Patient patient)
        {
            if (doctor.Preferences == null || !doctor.Preferences.Any())
            {
                return 0.5; // Neutral score if doctor has no preferences defined
            }

            double totalScore = 0;
            int relevantPreferenceCount = 0;

            foreach (var preference in doctor.Preferences)
            {
                double currentPrefScore = -1; // Indicates not evaluated yet for this preference rule

                try // Add try-catch for safety during preference evaluation
                {
                    if (preference.Type == PreferenceType.PatientComplexity && preference.LevelValue.HasValue)
                    {
                        relevantPreferenceCount++;
                        ComplexityLevel patientLevel = patient.ComplexityLevel;
                        ComplexityLevel prefLevel = (ComplexityLevel)preference.LevelValue.Value;

                        if (preference.Direction == PreferenceDirection.Prefers)
                            currentPrefScore = (patientLevel == prefLevel) ? 1.0 : 0.2; // High score for match, low otherwise
                        else // Avoids
                            currentPrefScore = (patientLevel == prefLevel) ? 0.0 : 0.8; // Low score for match, high otherwise
                    }
                    else if (preference.Type == PreferenceType.PatientUrgency && preference.LevelValue.HasValue)
                    {
                        relevantPreferenceCount++;
                        UrgencyLevel patientLevel = patient.Urgency;
                        UrgencyLevel prefLevel = (UrgencyLevel)preference.LevelValue.Value;

                        if (preference.Direction == PreferenceDirection.Prefers)
                            currentPrefScore = (patientLevel == prefLevel) ? 1.0 : 0.2;
                        else // Avoids
                            currentPrefScore = (patientLevel == prefLevel) ? 0.0 : 0.8;
                    }
                    else if (preference.Type == PreferenceType.PatientCondition && !string.IsNullOrEmpty(preference.ConditionValue))
                    {
                        relevantPreferenceCount++;
                        bool conditionMatch = (patient.Condition != null &&
                                              patient.Condition.Equals(preference.ConditionValue, StringComparison.OrdinalIgnoreCase));

                        if (preference.Direction == PreferenceDirection.Prefers)
                            currentPrefScore = conditionMatch ? 1.0 : 0.2;
                        else // Avoids
                            currentPrefScore = conditionMatch ? 0.0 : 0.8;
                    }
                }
                catch (Exception ex)
                {
                    // Log error if preference evaluation fails (e.g., bad LevelValue cast)
                    Console.WriteLine($"Error evaluating preference: {ex.Message}");
                    currentPrefScore = 0.5; // Assign neutral score on error
                    if (relevantPreferenceCount == 0) relevantPreferenceCount = 1; // Avoid division by zero if error on first pref
                }


                if (currentPrefScore >= 0) // If the preference was relevant and evaluated
                {
                    totalScore += currentPrefScore;
                }
                // If preference type wasn't relevant to this patient, relevantPreferenceCount is not incremented
            }

            // Return average score, defaulting to neutral if no relevant preferences applied
            return (relevantPreferenceCount == 0) ? 0.5 : totalScore / relevantPreferenceCount;
        }


        /// <summary>
        /// Helper to determine required experience based on patient urgency enum.
        /// </summary>
        private ExperienceLevel GetRequiredExperienceLevel(UrgencyLevel urgency)
        {
            switch (urgency)
            {
                case UrgencyLevel.High: return ExperienceLevel.Senior;
                case UrgencyLevel.Medium: return ExperienceLevel.Regular;
                default: return ExperienceLevel.Junior; // Low urgency
            }
        }

        /// <summary>
        /// Creates a deep clone of a schedule object.
        /// </summary>
        private Schedule CloneSchedule(Schedule original)
        {
            if (original == null) return new Schedule(); // Handle null input

            var clone = new Schedule
            {
                FitnessScore = original.FitnessScore // Copy fitness score too
            };
            // Deep copy dictionaries
            foreach (var kvp in original.DoctorToPatients)
            {
                clone.DoctorToPatients[kvp.Key] = new List<int>(kvp.Value);
            }
            foreach (var kvp in original.PatientToDoctor)
            {
                clone.PatientToDoctor[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        /// <summary>
        /// Checks if the termination conditions for the algorithm have been met.
        /// </summary>
        private bool TerminationConditionMet()
        {
            if (!Population.Any()) { Console.WriteLine("Termination: Population empty."); return true; } // Stop if population dies out
            if (currentGeneration >= maxGenerations) { Console.WriteLine("Termination: Max generations reached."); return true; }
            if (bestFitness >= fitnessThreshold) { Console.WriteLine("Termination: Fitness threshold reached."); return true; }
            if (stagnationCount >= maxStagnation) { Console.WriteLine("Termination: Stagnation detected."); return true; }
            return false;
        }

        /// <summary>
        /// Updates the best fitness score and stagnation count.
        /// </summary>
        private void UpdateStagnation(double currentBestFitness)
        {
            // Use a small tolerance for comparing floating-point fitness values
            double tolerance = 1e-6;
            if (currentBestFitness > bestFitness + tolerance)
            {
                // previousBestFitness = bestFitness; // Only needed if using relative improvement checks
                bestFitness = currentBestFitness;
                stagnationCount = 0; // Reset stagnation counter
                Console.WriteLine($"Generation {currentGeneration}: Improvement! Best fitness: {bestFitness:F2}");
                // Optional: Print details of the best schedule periodically
                if (currentGeneration % 50 == 0) PrintScheduleDetails(GetLeadingSchedule()); // Print less often
            }
            else
            {
                stagnationCount++;
                // Only print stagnation message periodically to avoid flooding console
                if (stagnationCount % 50 == 0 || stagnationCount == maxStagnation) // Print more selectively
                {
                    Console.WriteLine($"Generation {currentGeneration}: No significant improvement. Stagnation: {stagnationCount}/{maxStagnation}. Best Fitness: {bestFitness:F2}");
                }
            }
        }

        /// <summary>
        /// Calculates and caches the fitness score for all schedules in the population.
        /// </summary>
        private void CalculateFitnessForAll(List<Schedule> population)
        {


             Parallel.ForEach(population, schedule => 
            {
                
                schedule.FitnessScore = -1;
                ScoreSchedule(schedule); 
            });
        }


        /// <summary>
        /// Prints summary details of a given schedule (for debugging/logging).
        /// </summary>
        private void PrintScheduleDetails(Schedule schedule)
        {
            if (schedule == null) { Console.WriteLine("Cannot print details for null schedule."); return; }

            int assignedPatients = schedule.PatientToDoctor.Count;
            int totalPatientsNeedingDoctor = PatientsById.Values.Count(p => !p.NeedsSurgery);
            double assignmentPercentage = totalPatientsNeedingDoctor > 0 ? (double)assignedPatients / totalPatientsNeedingDoctor * 100 : 0;

            Console.WriteLine($"--- Schedule Details (Fitness: {schedule.FitnessScore:F2}) ---");
            Console.WriteLine($"Patients assigned: {assignedPatients}/{totalPatientsNeedingDoctor} ({assignmentPercentage:F1}%)");

            var currentWorkloads = RecalculateWorkloads(schedule);
            var doctorDetails = currentWorkloads.Keys
                .Select(id => DoctorsById.TryGetValue(id, out var doc) ? doc : null)
                .Where(doc => doc != null)
                .Select(doc => new {
                    Doctor = doc,
                    AssignedCount = currentWorkloads[doc.Id],
                    Percentage = doc.MaxWorkload > 0 ? (double)currentWorkloads[doc.Id] / doc.MaxWorkload * 100 : 0
                })
                .ToList();

            if (doctorDetails.Any())
            {
                double minWorkload = doctorDetails.Min(d => d.Percentage);
                double maxWorkload = doctorDetails.Max(d => d.Percentage);
                double avgWorkload = doctorDetails.Average(d => d.Percentage);
                // Calculate StdDev safely
                double variance = doctorDetails.Count > 1 ? doctorDetails.Sum(d => Math.Pow(d.Percentage - avgWorkload, 2)) / doctorDetails.Count : 0;
                double stdDeviation = Math.Sqrt(variance);

                Console.WriteLine($"Doctor workload - Min: {minWorkload:F1}%, Max: {maxWorkload:F1}%, Avg: {avgWorkload:F1}%, StdDev: {stdDeviation:F1}%");
            }
            else
            {
                Console.WriteLine("No doctors have assignments in this schedule.");
            }
            Console.WriteLine($"------------------------------------");
        }

        #endregion
    }
}
