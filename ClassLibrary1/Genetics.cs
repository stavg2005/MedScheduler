
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Genetics
    {
        #region Data
        private List<Schedule> Population { get; set; }
        private readonly int populationSize;
        private Random rnd = new Random();
        private List<Doctor> Doctors { get; set; }
        private List<Patient> Patients { get; set; }

        // Termination condition variables
        private int maxGenerations = 150; // Increased max generations
        private int currentGeneration = 0;
        private double fitnessThreshold = 2000; // Increased threshold for better solutions
        private int stagnationCount = 0;
        private int maxStagnation = 15; // Increased to allow more time to find optimal solution
        private double bestFitness = double.MinValue;
        private double previousBestFitness = double.MinValue;

        // Genetic algorithm parameters
        private double crossoverRate = 0.8; // 80% chance of crossover
        private double mutationRate = 0.2; // 20% chance of mutation
        private int tournamentSize = 5; // Number of schedules to consider in tournament selection

        // Weights for fitness function
        private readonly double specializationMatchWeight = 50.0;
        private readonly double urgencyWeight = 40.0;
        private readonly double workloadBalanceWeight = 30.0;
        private readonly double patientAssignmentWeight = 10.0;
        private readonly double continuityOfCareWeight = 15.0; // For maintaining doctor-patient relationships

        // Tracking previous assignments for continuity of care
        private Dictionary<int, int> previousAssignments = new Dictionary<int, int>(); // PatientId -> DoctorId

        public Genetics(int populationSize, List<Doctor> doctors, List<Patient> patients)
        {
            this.populationSize = populationSize;
            this.Doctors = doctors;
            this.Patients = patients;
        }
        #endregion

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
                int eliteCount = (int)(populationSize * 0.1); // 10% elitism
                newPopulation.AddRange(Population.OrderByDescending(ScoreSchedule).Take(eliteCount));

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
                }
                else
                {
                    stagnationCount++;
                    Console.WriteLine($"Generation {currentGeneration}: No improvement. Stagnation count: {stagnationCount}");
                }

                // Every 10 generations, print details about the best schedule
                if (currentGeneration % 10 == 0)
                {
                    Console.WriteLine("Best schedule details:");
                    var bestSchedule = GetLeadingSchedule();
                    PrintScheduleDetails(bestSchedule);
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

        private void PrintScheduleDetails(Schedule schedule)
        {
            int assignedPatients = schedule.PatientToDoctor.Count;
            int totalPatients = Patients.Count;
            double assignmentPercentage = (double)assignedPatients / totalPatients * 100;

            Console.WriteLine($"Patients assigned: {assignedPatients}/{totalPatients} ({assignmentPercentage:F1}%)");

            // Doctor workload distribution
            var doctorWorkloads = new Dictionary<int, int>();
            foreach (var doctorId in schedule.DoctorToPatients.Keys)
            {
                doctorWorkloads[doctorId] = schedule.DoctorToPatients[doctorId].Count;
            }

            int minWorkload = doctorWorkloads.Any() ? doctorWorkloads.Values.Min() : 0;
            int maxWorkload = doctorWorkloads.Any() ? doctorWorkloads.Values.Max() : 0;
            double avgWorkload = doctorWorkloads.Any() ? doctorWorkloads.Values.Average() : 0;

            Console.WriteLine($"Doctor workload - Min: {minWorkload}, Max: {maxWorkload}, Avg: {avgWorkload:F1}");

            // Specialization match percentage
            int correctSpecializationCount = 0;
            foreach (var pair in schedule.PatientToDoctor)
            {
                int patientId = pair.Key;
                int doctorId = pair.Value;

                var patient = Patients.First(p => p.Id == patientId);
                var doctor = Doctors.First(d => d.Id == doctorId);

                if (doctor.Specialization == patient.RequiredSpecialization)
                {
                    correctSpecializationCount++;
                }
            }

            double specializationMatchPercentage = assignedPatients > 0 ?
                (double)correctSpecializationCount / assignedPatients * 100 : 0;

            Console.WriteLine($"Specialization match: {correctSpecializationCount}/{assignedPatients} ({specializationMatchPercentage:F1}%)");
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

        private List<Schedule> GeneratePopulation(int size)
        {
            return Enumerable.Range(0, size)
                .Select(_ => GenerateRandomSchedule())
                .ToList();
        }

        private Schedule GenerateRandomSchedule()
        {
            var schedule = new Schedule
            {
                DoctorToPatients = new Dictionary<int, List<int>>(),
                PatientToDoctor = new Dictionary<int, int>()
            };

            // Reset doctor workloads
            foreach (var doctor in Doctors)
            {
                doctor.Workload = 0;
            }

            // Sort patients by urgency for prioritization
            var prioritizedPatients = Patients
                .OrderByDescending(p => GetUrgencyValue(p.Urgency))
                .ToList();

            foreach (var patient in prioritizedPatients)
            {
                // First try to find doctors with the right specialization
                var preferredDoctors = Doctors
                    .Where(d => d.Specialization == patient.RequiredSpecialization && d.Workload < d.MaxWorkload)
                    .ToList();

                // If no doctor with matching specialization is available, look for any available doctor
                if (!preferredDoctors.Any())
                {
                    preferredDoctors = Doctors
                        .Where(d => d.Workload < d.MaxWorkload)
                        .ToList();
                }

                // No available doctors at all
                if (!preferredDoctors.Any())
                {
                    continue;
                }

                // If there was a previous assignment for continuity of care, try to maintain it
                if (previousAssignments.ContainsKey(patient.Id))
                {
                    int previousDoctorId = previousAssignments[patient.Id];
                    var previousDoctor = Doctors.FirstOrDefault(d => d.Id == previousDoctorId && d.Workload < d.MaxWorkload);

                    if (previousDoctor != null)
                    {
                        AssignPatientToDoctor(schedule, patient.Id, previousDoctor.Id);
                        continue;
                    }
                }

                // Prioritize workload balance among qualified doctors
                var selectedDoctor = preferredDoctors
                    .OrderBy(d => d.Workload)
                    .First();

                AssignPatientToDoctor(schedule, patient.Id, selectedDoctor.Id);
            }

            return schedule;
        }

        private int GetUrgencyValue(string urgency)
        {
            switch (urgency.ToLower())
            {
                case "high": return 3;
                case "medium": return 2;
                case "low": return 1;
                default: return 0;
            }
        }

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
            foreach (var doctor in Doctors)
            {
                doctor.Workload = 0;
            }

            // Get all patients that have been assigned in either parent
            var allAssignedPatients = parent1.PatientToDoctor.Keys
                .Union(parent2.PatientToDoctor.Keys)
                .ToList();

            // Crossover point
            int crossoverPoint = rnd.Next(allAssignedPatients.Count);

            // For each patient, decide which parent's assignment to use
            for (int i = 0; i < allAssignedPatients.Count; i++)
            {
                int patientId = allAssignedPatients[i];

                // First offspring takes assignments from parent1 up to crossover point, then from parent2
                if (i < crossoverPoint)
                {
                    if (parent1.PatientToDoctor.ContainsKey(patientId))
                    {
                        int doctorId = parent1.PatientToDoctor[patientId];
                        var doctor = Doctors.First(d => d.Id == doctorId);

                        if (doctor.Workload < doctor.MaxWorkload)
                        {
                            AssignPatientToDoctor(offspring1, patientId, doctorId);
                        }
                    }

                    if (parent2.PatientToDoctor.ContainsKey(patientId))
                    {
                        int doctorId = parent2.PatientToDoctor[patientId];
                        var doctor = Doctors.First(d => d.Id == doctorId);

                        if (doctor.Workload < doctor.MaxWorkload)
                        {
                            AssignPatientToDoctor(offspring2, patientId, doctorId);
                        }
                    }
                }
                else
                {
                    if (parent2.PatientToDoctor.ContainsKey(patientId))
                    {
                        int doctorId = parent2.PatientToDoctor[patientId];
                        var doctor = Doctors.First(d => d.Id == doctorId);

                        if (doctor.Workload < doctor.MaxWorkload)
                        {
                            AssignPatientToDoctor(offspring1, patientId, doctorId);
                        }
                    }

                    if (parent1.PatientToDoctor.ContainsKey(patientId))
                    {
                        int doctorId = parent1.PatientToDoctor[patientId];
                        var doctor = Doctors.First(d => d.Id == doctorId);

                        if (doctor.Workload < doctor.MaxWorkload)
                        {
                            AssignPatientToDoctor(offspring2, patientId, doctorId);
                        }
                    }
                }
            }

            // Assign any unassigned high urgency patients
            AssignRemainingUrgentPatients(offspring1);
            AssignRemainingUrgentPatients(offspring2);

            return new List<Schedule> { offspring1, offspring2 };
        }

        private void AssignRemainingUrgentPatients(Schedule schedule)
        {
            var unassignedUrgentPatients = Patients
                .Where(p => GetUrgencyValue(p.Urgency) > 1 && !schedule.PatientToDoctor.ContainsKey(p.Id))
                .OrderByDescending(p => GetUrgencyValue(p.Urgency))
                .ToList();

            foreach (var patient in unassignedUrgentPatients)
            {
                var availableDoctors = Doctors
                    .Where(d => d.Specialization == patient.RequiredSpecialization && d.Workload < d.MaxWorkload)
                    .OrderBy(d => d.Workload)
                    .ToList();

                if (!availableDoctors.Any())
                {
                    availableDoctors = Doctors
                        .Where(d => d.Workload < d.MaxWorkload)
                        .OrderBy(d => d.Workload)
                        .ToList();
                }

                if (availableDoctors.Any())
                {
                    AssignPatientToDoctor(schedule, patient.Id, availableDoctors.First().Id);
                }
            }
        }

        private void Mutate(Schedule schedule)
        {
            // Mutation types:
            // 1. Reassign a patient to a different doctor
            // 2. Swap assignments between two patients
            // 3. Add a previously unassigned patient

            int mutationType = rnd.Next(3);

            switch (mutationType)
            {
                case 0: // Reassign
                    MutateReassignPatient(schedule);
                    break;
                case 1: // Swap
                    MutateSwapPatients(schedule);
                    break;
                case 2: // Add new
                    MutateAddNewPatient(schedule);
                    break;
            }
        }

        private void MutateReassignPatient(Schedule schedule)
        {
            if (!schedule.PatientToDoctor.Any()) return;

            // Select a random patient
            int patientIndex = rnd.Next(schedule.PatientToDoctor.Count);
            int patientId = schedule.PatientToDoctor.Keys.ElementAt(patientIndex);
            var patient = Patients.First(p => p.Id == patientId);

            // Find available doctors with the right specialization
            var availableDoctors = Doctors
                .Where(d => d.Specialization == patient.RequiredSpecialization &&
                            d.Id != schedule.PatientToDoctor[patientId] &&
                            d.Workload < d.MaxWorkload)
                .ToList();

            if (!availableDoctors.Any())
            {
                // If no specialist is available, try any available doctor
                availableDoctors = Doctors
                    .Where(d => d.Id != schedule.PatientToDoctor[patientId] &&
                                d.Workload < d.MaxWorkload)
                    .ToList();
            }

            if (availableDoctors.Any())
            {
                // Remove current assignment
                int currentDoctorId = schedule.PatientToDoctor[patientId];
                schedule.DoctorToPatients[currentDoctorId].Remove(patientId);
                if (!schedule.DoctorToPatients[currentDoctorId].Any())
                {
                    schedule.DoctorToPatients.Remove(currentDoctorId);
                }

                var currentDoctor = Doctors.First(d => d.Id == currentDoctorId);
                currentDoctor.Workload--;

                // Assign to new doctor
                int newDoctorId = availableDoctors[rnd.Next(availableDoctors.Count)].Id;
                AssignPatientToDoctor(schedule, patientId, newDoctorId);
            }
        }

        private void MutateSwapPatients(Schedule schedule)
        {
            if (schedule.PatientToDoctor.Count < 2) return;

            // Select two random patients
            int patientIndex1 = rnd.Next(schedule.PatientToDoctor.Count);
            int patientIndex2 = rnd.Next(schedule.PatientToDoctor.Count);

            // Try to get different patients
            while (patientIndex2 == patientIndex1 && schedule.PatientToDoctor.Count > 1)
            {
                patientIndex2 = rnd.Next(schedule.PatientToDoctor.Count);
            }

            int patientId1 = schedule.PatientToDoctor.Keys.ElementAt(patientIndex1);
            int patientId2 = schedule.PatientToDoctor.Keys.ElementAt(patientIndex2);

            int doctorId1 = schedule.PatientToDoctor[patientId1];
            int doctorId2 = schedule.PatientToDoctor[patientId2];

            // Only swap if doctors are different
            if (doctorId1 != doctorId2)
            {
                var doctor1 = Doctors.First(d => d.Id == doctorId1);
                var doctor2 = Doctors.First(d => d.Id == doctorId2);

                // Remove current assignments
                schedule.DoctorToPatients[doctorId1].Remove(patientId1);
                schedule.DoctorToPatients[doctorId2].Remove(patientId2);

                // Swap assignments
                schedule.DoctorToPatients[doctorId1].Add(patientId2);
                schedule.DoctorToPatients[doctorId2].Add(patientId1);

                // Update patient to doctor mappings
                schedule.PatientToDoctor[patientId1] = doctorId2;
                schedule.PatientToDoctor[patientId2] = doctorId1;
            }
        }

        private void MutateAddNewPatient(Schedule schedule)
        {
            // Find unassigned patients
            var unassignedPatients = Patients
                .Where(p => !schedule.PatientToDoctor.ContainsKey(p.Id))
                .ToList();

            if (!unassignedPatients.Any()) return;

            // Select a random unassigned patient
            var patient = unassignedPatients[rnd.Next(unassignedPatients.Count)];

            // Find available doctors with the right specialization
            var availableDoctors = Doctors
                .Where(d => d.Specialization == patient.RequiredSpecialization &&
                            d.Workload < d.MaxWorkload)
                .ToList();

            if (!availableDoctors.Any())
            {
                // If no specialist is available, try any available doctor
                availableDoctors = Doctors
                    .Where(d => d.Workload < d.MaxWorkload)
                    .ToList();
            }

            if (availableDoctors.Any())
            {
                int doctorId = availableDoctors[rnd.Next(availableDoctors.Count)].Id;
                AssignPatientToDoctor(schedule, patient.Id, doctorId);
            }
        }

        private void AssignPatientToDoctor(Schedule schedule, int patientId, int doctorId)
        {
            // If the patient is already assigned to a doctor, remove them from that doctor's list
            if (schedule.PatientToDoctor.ContainsKey(patientId))
            {
                int previousDoctorId = schedule.PatientToDoctor[patientId];
                if (schedule.DoctorToPatients.ContainsKey(previousDoctorId))
                {
                    schedule.DoctorToPatients[previousDoctorId].Remove(patientId);

                    // Clean up empty lists
                    if (!schedule.DoctorToPatients[previousDoctorId].Any())
                    {
                        schedule.DoctorToPatients.Remove(previousDoctorId);
                    }

                    // Update the previous doctor's workload
                    var previousDoctor = Doctors.First(d => d.Id == previousDoctorId);
                    previousDoctor.Workload = Math.Max(0, previousDoctor.Workload - 1);
                }
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
            var doctor = Doctors.First(d => d.Id == doctorId);
            doctor.Workload++;
        }

        private double ScoreSchedule(Schedule schedule)
        {
            double score = 0;

            // Calculate scores for each doctor-patient assignment
            foreach (var doctorId in schedule.DoctorToPatients.Keys)
            {
                var doctor = Doctors.First(d => d.Id == doctorId);
                var patients = schedule.DoctorToPatients[doctorId];

                // Base score for each patient assigned
                score += patients.Count * patientAssignmentWeight;

                // Penalty for high workload (non-linear to discourage very high workloads)
                double workloadFactor = (double)doctor.Workload / doctor.MaxWorkload;
                double workloadScore = (1 - Math.Pow(workloadFactor, 2)) * workloadBalanceWeight * patients.Count;
                score += workloadScore;

                // Score for each patient
                foreach (var patientId in patients)
                {
                    var patient = Patients.First(p => p.Id == patientId);

                    // Specialization match bonus
                    if (doctor.Specialization == patient.RequiredSpecialization)
                    {
                        score += specializationMatchWeight;
                    }

                    // Urgency handling bonus
                    switch (patient.Urgency.ToLower())
                    {
                        case "high":
                            score += urgencyWeight;
                            break;
                        case "medium":
                            score += urgencyWeight * 0.6;
                            break;
                        case "low":
                            score += urgencyWeight * 0.3;
                            break;
                    }

                    // Continuity of care bonus
                    if (previousAssignments.ContainsKey(patientId) && previousAssignments[patientId] == doctorId)
                    {
                        score += continuityOfCareWeight;
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

        private Schedule GetLeadingSchedule()
        {
            return Population
                .OrderByDescending(ScoreSchedule)
                .FirstOrDefault();
        }
    }
}