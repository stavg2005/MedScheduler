using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace MedScheduler
{
    public class DataGenerator
    {
        private Random random = new Random();

        // Lists for random data generation
        private List<string> firstNames = new List<string> {
            "John", "Sarah", "Michael", "Emma", "David", "Jennifer", "Robert", "Emily",
            "William", "Olivia", "James", "Sophia", "Benjamin", "Ava", "Daniel", "Isabella",
            "Matthew", "Mia", "Joseph", "Charlotte", "Andrew", "Amelia", "Samuel", "Harper",
            "Anthony", "Abigail", "Christopher", "Elizabeth", "Ryan", "Sofia", "Nathan", "Ella"
        };

        private List<string> lastNames = new List<string> {
            "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson",
            "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin",
            "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee",
            "Walker", "Hall", "Allen", "Young", "Hernandez", "King", "Wright", "Lopez"
        };

        private List<string> specializations = new List<string> {
            "Cardiology", "Neurology", "Orthopedics", "Pediatrics", "Dermatology", "Oncology",
            "Emergency Medicine", "Pulmonology", "Gastroenterology", "Endocrinology",
            "Rheumatology", "Ophthalmology", "Psychiatry", "Urology", "Nephrology",
            "Geriatrics", "Infectious Disease", "Hematology", "OB/GYN", "General Surgery"
        };

        private List<string> conditions = new List<string> {
            "Heart Disease", "Migraine", "Broken Leg", "Pneumonia", "Eczema", "Lung Cancer",
            "Chest Pain", "COPD", "Ulcerative Colitis", "Type 1 Diabetes", "Rheumatoid Arthritis",
            "Cataracts", "Depression", "Kidney Stones", "Chronic Kidney Disease", "Dementia",
            "HIV", "Anemia", "Pregnancy Complications", "Appendicitis"
        };

        private List<string> urgencies = new List<string> { "High", "Medium", "Low" };

        private List<string> equipments = new List<string> {
            "X-Ray Machine", "MRI Scanner", "CT Scanner", "Ultrasound", "EKG Machine",
            "Anesthesia Machine", "Vital Signs Monitor", "Defibrillator", "Ventilator",
            "Surgical Laser", "Endoscope", "Surgical Microscope", "Dialysis Machine"
        };

        public List<Doctor> GenerateDoctors(int count)
        {
            List<Doctor> doctors = new List<Doctor>();

            for (int i = 1; i <= count; i++)
            {
                string specialization = specializations[random.Next(specializations.Count)];
                int maxWorkload = random.Next(5, 16); // Random workload between 5-15
                int experienceLevel = random.Next(1, 4); // 1-Junior, 2-Regular, 3-Senior

                Doctor doctor = new Doctor
                {
                    Id = i,
                    Name = $"Dr. {firstNames[random.Next(firstNames.Count)]} {lastNames[random.Next(lastNames.Count)]}",
                    Specialization = specialization,
                    Workload = 0,
                    MaxWorkload = maxWorkload,
                    ExperienceLevel = experienceLevel,
                    Preferences = GenerateRandomPreferences()
                };

                doctors.Add(doctor);
            }

            return doctors;
        }

        public List<Patient> GeneratePatients(int count, List<Doctor> doctors)
        {
            List<Patient> patients = new List<Patient>();

            for (int i = 1; i <= count; i++)
            {
                string specialization = specializations[random.Next(specializations.Count)];
                string condition = conditions[random.Next(conditions.Count)];
                string urgency = urgencies[random.Next(urgencies.Count)];
                bool needsSurgery = random.Next(100) < 30; // 30% chance to need surgery
                int complexityLevel = random.Next(1, 4); // 1-Simple, 2-Moderate, 3-Complex

                Patient patient = new Patient
                {
                    Id = i,
                    Name = $"{firstNames[random.Next(firstNames.Count)]} {lastNames[random.Next(lastNames.Count)]}",
                    Condition = condition,
                    Urgency = urgency,
                    RequiredSpecialization = specialization,
                    NeedsSurgery = needsSurgery,
                    AdmissionDate = DateTime.Now.AddDays(-random.Next(0, 10)),
                    ComplexityLevel = complexityLevel,
                    EstimatedTreatmentTime = random.Next(1, 5) + random.NextDouble() // 1-5 hours with decimal
                };

                // Add previous doctors for some patients (continuity of care)
                if (random.Next(100) < 40) // 40% chance
                {
                    int previousDoctorCount = random.Next(1, 3);
                    for (int j = 0; j < previousDoctorCount; j++)
                    {
                        var doctor = doctors.Where(d => d.Specialization == patient.RequiredSpecialization)
                                           .Skip(random.Next(doctors.Count(d => d.Specialization == patient.RequiredSpecialization)))
                                           .FirstOrDefault();
                        if (doctor != null && !patient.PreviousDoctors.Contains(doctor.Id))
                        {
                            patient.PreviousDoctors.Add(doctor.Id);
                        }
                    }
                }

                patients.Add(patient);
            }

            return patients;
        }

        public List<MedicalProcedure> GenerateProcedures(int count)
        {
            List<MedicalProcedure> procedures = new List<MedicalProcedure>();
            List<string> procedureNames = new List<string>
            {
                "Appendectomy", "Coronary Bypass", "Hip Replacement", "Cataract Surgery",
                "Hernia Repair", "Cholecystectomy", "Hysterectomy", "Mastectomy", "Biopsy",
                "Angioplasty", "Colonoscopy", "Endoscopy", "Tonsillectomy", "Cesarean Section"
            };

            for (int i = 1; i <= count; i++)
            {
                string specialization = specializations[random.Next(specializations.Count)];
                bool isOperation = random.Next(100) < 70; // 70% chance of being an operation
                int complexityLevel = random.Next(1, 4); // 1-Simple, 2-Moderate, 3-Complex

                MedicalProcedure procedure = new MedicalProcedure
                {
                    Id = i,
                    Name = procedureNames[random.Next(procedureNames.Count)],
                    RequiredSpecialization = specialization,
                    EstimatedDuration = random.Next(1, 5) + random.NextDouble(), // 1-5 hours with decimal
                    IsOperation = isOperation,
                    ComplexityLevel = complexityLevel,
                    MinimumDoctorExperienceLevel = complexityLevel,
                    RequiredEquipment = GenerateRandomEquipment(random.Next(1, 4))
                };

                procedures.Add(procedure);
            }

            return procedures;
        }

        public List<OperatingRoom> GenerateOperatingRooms(int count)
        {
            List<OperatingRoom> operatingRooms = new List<OperatingRoom>();

            for (int i = 1; i <= count; i++)
            {
                bool isSpecialized = random.Next(100) < 40; // 40% chance of specialized rooms
                string specialization = isSpecialized ?
                    specializations[random.Next(specializations.Count)] : null;

                OperatingRoom operatingRoom = new OperatingRoom
                {
                    Id = i,
                    Name = $"OR-{i:D2}",
                    IsSpecialized = isSpecialized,
                    Specialization = specialization,
                    AvailableEquipment = GenerateRandomEquipment(random.Next(5, 10)),
                    AvailabilityHours = GenerateAvailabilityHours()
                };

                operatingRooms.Add(operatingRoom);
            }

            return operatingRooms;
        }

        public Schedule GenerateInitialSchedule(List<Doctor> doctors, List<Patient> patients)
        {
            Schedule schedule = new Schedule();

            // Assign some patients to doctors
            foreach (var patient in patients)
            {
                // Find doctors with matching specialization and available capacity
                var availableDoctors = doctors
                    .Where(d => d.Specialization == patient.RequiredSpecialization && d.CanAcceptPatient())
                    .ToList();

                if (availableDoctors.Any())
                {
                    // Randomly select a doctor
                    var doctor = availableDoctors[random.Next(availableDoctors.Count)];

                    // Assign patient to doctor
                    schedule.AssignPatientToDoctor(patient.Id, doctor.Id);

                    // Update doctor's workload
                    doctor.Workload++;
                }
            }

            return schedule;
        }

        private List<string> GenerateRandomPreferences()
        {
            List<string> preferences = new List<string>();
            int preferencesCount = random.Next(0, 4); // 0-3 preferences

            var preferenceItems = new List<string> {
                "Prefers complex cases", "Prefers elder patients", "Prefers children",
                "Prefers short procedures", "Prefers non-critical cases", "Prefers morning shifts",
                "Prefers afternoon shifts", "Willing to handle emergencies"
            };

            // Select random preferences without duplicates
            for (int i = 0; i < preferencesCount; i++)
            {
                string preference = preferenceItems[random.Next(preferenceItems.Count)];
                if (!preferences.Contains(preference))
                {
                    preferences.Add(preference);
                }
            }

            return preferences;
        }

        private List<string> GenerateRandomEquipment(int count)
        {
            List<string> selectedEquipment = new List<string>();

            // Ensure no duplicates in equipment
            var shuffledEquipment = equipments.OrderBy(_ => random.Next()).ToList();

            for (int i = 0; i < Math.Min(count, shuffledEquipment.Count); i++)
            {
                selectedEquipment.Add(shuffledEquipment[i]);
            }

            return selectedEquipment;
        }

        private Dictionary<DayOfWeek, List<TimeSpan>> GenerateAvailabilityHours()
        {
            var availability = new Dictionary<DayOfWeek, List<TimeSpan>>();

            // Generate availability for weekdays (Monday-Friday)
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday)
                {
                    availability[day] = new List<TimeSpan>();

                    // Operating rooms typically available 8AM-8PM
                    for (int hour = 8; hour < 20; hour += 2) // 2-hour blocks
                    {
                        // 80% chance of availability for each block
                        if (random.Next(100) < 80)
                        {
                            availability[day].Add(new TimeSpan(hour, 0, 0));
                        }
                    }
                }
                else
                {
                    // Limited weekend availability (10AM-4PM)
                    // Only add if random check passes (30% chance of weekend availability)
                    if (random.Next(100) < 30)
                    {
                        availability[day] = new List<TimeSpan>();

                        for (int hour = 10; hour < 16; hour += 2)
                        {
                            if (random.Next(100) < 50) // 50% chance per time slot
                            {
                                availability[day].Add(new TimeSpan(hour, 0, 0));
                            }
                        }
                    }
                }
            }

            return availability;
        }
    }
}