using MedScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedScheduler.Models;
namespace MedScheduler
{
    internal class Genetics
    {
        #region Data
        List<Schedule> Population { get; set; }
        readonly int pop_size;
        Random rnd = new Random();
        List<Doctor> Doctors { get; set; }
        List<Patient> Patients { get; set; }

        // Termination condition variables
        private int maxGenerations = 100; // Maximum number of generations
        private int currentGeneration = 0; // Current generation counter
        private double fitnessThreshold = 1000; // Fitness score threshold to stop
        private int stagnationCount = 0; // Count of generations without improvement
        private int maxStagnation = 10; // Maximum allowed stagnation before stopping
        private double bestFitness = double.MinValue; // Track the best fitness score

        public Genetics(int pop_size, List<Doctor> doctors, List<Patient> patients)
        {
            this.pop_size = pop_size;
            this.Doctors = doctors;
            this.Patients = patients;
        }

        #endregion

        public Schedule Solve()
        {
            Population = GeneratePopulation(pop_size);
            while (!TerminationConditionMet())
            {
                currentGeneration++;
                var group = QualityGroup((int)Math.Sqrt(pop_size / 2));
                Population = group
                    .SelectMany(firstSchedule => group
                    .SelectMany(secondSchedule => GenerateOffsprings(firstSchedule, secondSchedule)))
                    .ToList();

                // Track the best fitness in the current generation
                var currentBestFitness = Population.Max(ScoreSchedule);
                if (currentBestFitness > bestFitness)
                {
                    bestFitness = currentBestFitness;
                    stagnationCount = 0; // Reset stagnation counter
                }
                else
                {
                    stagnationCount++; // Increment stagnation counter
                }

                Console.WriteLine($"Generation {currentGeneration}, Best Fitness: {bestFitness}");
            }
            return GetLeadingSchedule();
        }

        bool TerminationConditionMet()
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

        List<Schedule> GeneratePopulation(int pop_size)
        {
            return Enumerable.Range(0, pop_size)
                .Select(_ => GenerateRandomSchedule())
                .ToList();
        }

        Schedule GenerateRandomSchedule()
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

            Console.WriteLine("Generating random schedule:");

            // Assign patients to doctors
            foreach (var patient in Patients.OrderBy(p => p.Urgency)) // Prioritize high-urgency patients
            {
                var availableDoctors = Doctors
                    .Where(d => d.Specialization == patient.RequiredSpecialization && d.Workload < d.MaxWorkload)
                    .OrderBy(d => d.Workload) // Prefer less busy doctors
                    .ToList();

                if (availableDoctors.Any())
                {
                    var doctor = availableDoctors[rnd.Next(availableDoctors.Count)];
                    if (!schedule.DoctorToPatients.ContainsKey(doctor.Id))
                    {
                        schedule.DoctorToPatients[doctor.Id] = new List<int>();
                    }
                    schedule.DoctorToPatients[doctor.Id].Add(patient.Id);
                    schedule.PatientToDoctor[patient.Id] = doctor.Id;
                    doctor.Workload++;

                    Console.WriteLine($"  Patient {patient.Id} (Urgency: {patient.Urgency}) assigned to Doctor {doctor.Id} (Specialization: {doctor.Specialization})");
                }
                else
                {
                    Console.WriteLine($"  No available doctor for Patient {patient.Id} (Required Specialization: {patient.RequiredSpecialization})");
                }
            }

            return schedule;
        }

        List<Schedule> QualityGroup(int size)
        {
            return Population
                .OrderByDescending(ScoreSchedule)
                .Take(size)
                .ToList();
        }

        int ScoreSchedule(Schedule schedule)
        {
            int score = 0;
            Console.WriteLine("Evaluating schedule:");

            foreach (var doctorId in schedule.DoctorToPatients.Keys)
            {
                var doctor = Doctors.First(d => d.Id == doctorId);
                var patients = schedule.DoctorToPatients[doctorId];

                Console.WriteLine($"Doctor {doctor.Id} (Specialization: {doctor.Specialization}) is assigned to patients: {string.Join(", ", patients)}");

                // Base score for each patient assigned
                score += patients.Count * 10;

                // Penalize for high workload, but cap the penalty
                int workloadPenalty = Math.Min(doctor.Workload * 2, 50); // Cap penalty at 50
                score -= workloadPenalty;

                // Reward for matching specialization
                foreach (var patientId in patients)
                {
                    var patient = Patients.First(p => p.Id == patientId);
                    if (doctor.Specialization == patient.RequiredSpecialization)
                    {
                        score += 50; // Increased bonus for correct specialization
                        Console.WriteLine($"  Patient {patient.Id} matches doctor's specialization (+50)");
                    }

                    // Reward for handling high-urgency patients
                    if (patient.Urgency == "High")
                    {
                        score += 50; // Increased bonus for high-urgency patients
                        Console.WriteLine($"  Patient {patient.Id} is high urgency (+50)");
                    }
                    else if (patient.Urgency == "Medium")
                    {
                        score += 30; // Increased bonus for medium-urgency patients
                        Console.WriteLine($"  Patient {patient.Id} is medium urgency (+30)");
                    }
                    else
                    {
                        score += 20; // Increased bonus for low-urgency patients
                        Console.WriteLine($"  Patient {patient.Id} is low urgency (+20)");
                    }
                }
            }

            Console.WriteLine($"Total score for schedule: {score}");
            return score;
        }

        void AssignPatientToDoctor(Schedule schedule, int patientId, int doctorId)
        {
            // If the patient is already assigned to a doctor, remove them from that doctor's list
            if (schedule.PatientToDoctor.ContainsKey(patientId))
            {
                int previousDoctorId = schedule.PatientToDoctor[patientId];
                schedule.DoctorToPatients[previousDoctorId].Remove(patientId);

                // Update the previous doctor's workload
                var previousDoctor = Doctors.First(d => d.Id == previousDoctorId);
                previousDoctor.Workload--;
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
        List<Schedule> GenerateOffsprings(Schedule dad, Schedule mom)
        {
            var offspring = new Schedule
            {
                DoctorToPatients = new Dictionary<int, List<int>>(),
                PatientToDoctor = new Dictionary<int, int>()
            };

            Console.WriteLine("Generating offspring:");

            // Crossover: Combine dad and mom
            foreach (var patient in Patients)
            {
                if (rnd.NextDouble() < 0.5)
                {
                    if (dad.PatientToDoctor.ContainsKey(patient.Id))
                    {
                        AssignPatientToDoctor(offspring, patient.Id, dad.PatientToDoctor[patient.Id]);
                        Console.WriteLine($"  Patient {patient.Id} assigned to Doctor {dad.PatientToDoctor[patient.Id]} (from dad)");
                    }
                }
                else
                {
                    if (mom.PatientToDoctor.ContainsKey(patient.Id))
                    {
                        AssignPatientToDoctor(offspring, patient.Id, mom.PatientToDoctor[patient.Id]);
                        Console.WriteLine($"  Patient {patient.Id} assigned to Doctor {mom.PatientToDoctor[patient.Id]} (from mom)");
                    }
                }
            }

            // Mutation: Randomly reassign some patients
            foreach (var patient in Patients)
            {
                if (rnd.NextDouble() < 0.1) // 10% mutation rate
                {
                    var availableDoctors = Doctors
                        .Where(d => d.Specialization == patient.RequiredSpecialization && d.Workload < d.MaxWorkload)
                        .ToList();

                    if (availableDoctors.Any())
                    {
                        var newDoctor = availableDoctors[rnd.Next(availableDoctors.Count)];
                        AssignPatientToDoctor(offspring, patient.Id, newDoctor.Id);
                        Console.WriteLine($"  Patient {patient.Id} reassigned to Doctor {newDoctor.Id} (mutation)");
                    }
                }
            }

            return new List<Schedule> { offspring };
        }

        Schedule GetLeadingSchedule()
        {
            return Population
                .OrderByDescending(ScoreSchedule)
                .FirstOrDefault();
        }
    }
}
