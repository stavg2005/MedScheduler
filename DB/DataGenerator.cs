using System;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary1;
using Models; // Assuming all models are here

namespace MedScheduler // Or your appropriate namespace
{
    public class DataGenerator
    {
        private Random random = new Random();

        // --- Base Data for Correlation ---

        // Define some specializations, separating surgical ones
        private List<string> medicalSpecializations = new List<string> {
            "Cardiology", "Neurology", "Pediatrics", "Dermatology", "Oncology",
            "Pulmonology", "Gastroenterology", "Endocrinology", "Rheumatology",
            "Psychiatry", "Nephrology", "Geriatrics", "Infectious Disease", "Hematology"
        };
        private List<string> surgicalSpecializations = new List<string> {
            "General Surgery", "Orthopedics", "Neurosurgery", "Ophthalmology", "Urology", "OB/GYN", "Cardiac Surgery" // Added Cardiac
        };
        private List<string> allSpecializations => medicalSpecializations.Concat(surgicalSpecializations).ToList();

        // Define sample procedures mapped to specializations
        private Dictionary<string, List<string>> proceduresBySpecialization = new Dictionary<string, List<string>> {
            { "General Surgery", new List<string> { "Appendectomy", "Hernia Repair", "Cholecystectomy" } },
            { "Orthopedics", new List<string> { "Hip Replacement", "Knee Replacement", "ACL Reconstruction", "Rotator Cuff Repair" } },
            { "Neurosurgery", new List<string> { "Spinal Fusion", "Craniotomy" } },
            { "Ophthalmology", new List<string> { "Cataract Surgery" } },
            { "Urology", new List<string> { "Kidney Stone Removal (Lithotripsy)", "Prostatectomy" } },
            { "OB/GYN", new List<string> { "Hysterectomy", "Cesarean Section" } },
            { "Cardiac Surgery", new List<string> { "Coronary Artery Bypass Graft (CABG)", "Valve Replacement" } },
            { "Cardiology", new List<string> { "Angioplasty", "Stent Placement", "Cardiac Catheterization", "Pacemaker Implantation" } }, // Some non-surgical
            { "Gastroenterology", new List<string> { "Colonoscopy", "Endoscopy (EGD)" } },
            { "Oncology", new List<string> { "Biopsy", "Lumpectomy", "Chemotherapy Administration" } }
            // Add more mappings...
        };

        // Define sample equipment needs (can be more granular)
        private Dictionary<string, List<string>> equipmentBySpecialization = new Dictionary<string, List<string>> {
            { "General Surgery", new List<string> { "Surgical Laser", "Endoscope", "Vital Signs Monitor", "Anesthesia Machine" } },
            { "Orthopedics", new List<string> { "X-Ray Machine", "Surgical Microscope", "Vital Signs Monitor", "Anesthesia Machine" } },
            { "Cardiac Surgery", new List<string> { "Heart-Lung Machine", "EKG Machine", "Defibrillator", "Vital Signs Monitor", "Anesthesia Machine" } },
            { "Cardiology", new List<string> { "EKG Machine", "Angiography Suite", "Ultrasound" } },
            { "Radiology", new List<string> { "X-Ray Machine", "MRI Scanner", "CT Scanner", "Ultrasound" } } // Example non-scheduling spec
            // Add more mappings...
        };
        private List<string> basicOrEquipment = new List<string> { "Vital Signs Monitor", "Anesthesia Machine", "Defibrillator" };
        private List<string> allEquipment = new List<string> {
            "X-Ray Machine", "MRI Scanner", "CT Scanner", "Ultrasound", "EKG Machine",
            "Anesthesia Machine", "Vital Signs Monitor", "Defibrillator", "Ventilator",
            "Surgical Laser", "Endoscope", "Surgical Microscope", "Dialysis Machine",
            "Heart-Lung Machine", "Angiography Suite", "Laparoscopic Tower"
        };


        // Keep names lists
        private List<string> firstNames = new List<string> { "John", "Sarah", "Michael", "Emma", "David", "Jennifer", "Robert", "Emily", "William", "Olivia", "James", "Sophia", "Benjamin", "Ava", "Daniel", "Isabella", "Matthew", "Mia", "Joseph", "Charlotte", "Andrew", "Amelia", "Samuel", "Harper", "Anthony", "Abigail", "Christopher", "Elizabeth", "Ryan", "Sofia", "Nathan", "Ella" };
        private List<string> lastNames = new List<string> { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "Hernandez", "King", "Wright", "Lopez" };
        private List<string> conditions = new List<string> { "Heart Disease", "Migraine", "Broken Leg", "Pneumonia", "Eczema", "Lung Cancer", "Chest Pain", "COPD", "Ulcerative Colitis", "Type 1 Diabetes", "Rheumatoid Arthritis", "Cataracts", "Depression", "Kidney Stones", "Chronic Kidney Disease", "Dementia", "HIV", "Anemia", "Pregnancy Complications", "Appendicitis", "Gallstones", "ACL Tear" };


        // --- Enhanced Generation Methods ---

        /// <summary>
        /// Generates Procedures first, ensuring they align with defined specializations.
        /// </summary>
        public List<MedicalProcedure> GenerateProcedures(int count)
        {
            List<MedicalProcedure> procedures = new List<MedicalProcedure>();
            List<string> availableSpecs = proceduresBySpecialization.Keys.ToList();

            for (int i = 1; i <= count; i++)
            {
                // Pick a specialization that has defined procedures
                string specialization = availableSpecs[random.Next(availableSpecs.Count)];
                string procedureName = proceduresBySpecialization[specialization][random.Next(proceduresBySpecialization[specialization].Count)];
                bool isOperation = surgicalSpecializations.Contains(specialization); // Assume surgical specs mean operation
                int complexityLevelInt = random.Next(1, 4);
                ExperienceLevel minExperience = (ExperienceLevel)complexityLevelInt; // Simple mapping: 1->Junior, 2->Regular, 3->Senior

                // Get relevant equipment
                List<string> requiredEquipment = new List<string>();
                if (equipmentBySpecialization.TryGetValue(specialization, out var specEquip))
                {
                    // Take a subset of the typical equipment for this spec
                    int equipCount = random.Next(1, specEquip.Count + 1);
                    requiredEquipment.AddRange(specEquip.OrderBy(x => random.Next()).Take(equipCount));
                }

                MedicalProcedure procedure = new MedicalProcedure
                {
                    Id = i,
                    Name = procedureName,
                    RequiredSpecialization = specialization,
                    EstimatedDuration = random.Next(1, 5) + Math.Round(random.NextDouble(), 1),
                    IsOperation = isOperation,
                    ComplexityLevel = complexityLevelInt,
                    MinimumDoctorExperienceLevel = minExperience,
                    RequiredEquipment = requiredEquipment.Distinct().ToList() // Ensure unique
                };
                procedures.Add(procedure);
            }
            Console.WriteLine($"Generated {procedures.Count} procedures.");
            return procedures;
        }

        /// <summary>
        /// Generates Doctors and Surgeons, ensuring coverage for specializations.
        /// </summary>
        public List<Doctor> GenerateDoctors(int count, List<MedicalProcedure> procedures)
        {
            List<Doctor> doctors = new List<Doctor>();
            var specsWithProcedures = procedures.Select(p => p.RequiredSpecialization).Distinct().ToList();
            var specsRequiringSurgery = procedures.Where(p => p.IsOperation).Select(p => p.RequiredSpecialization).Distinct().ToList();
            int currentId = 1;

            // Ensure at least a few doctors per specialization that has procedures
            foreach (string spec in specsWithProcedures)
            {
                int numDoctorsForSpec = random.Next(2, 6); // Generate 2-5 doctors per needed spec
                for (int i = 0; i < numDoctorsForSpec; i++)
                {
                    if (currentId > count) break; // Stop if we reach the total count

                    bool isSurgicalSpec = specsRequiringSurgery.Contains(spec);
                    // Higher chance of being a surgeon if it's a surgical spec
                    bool makeSurgeon = isSurgicalSpec && random.Next(100) < 70;

                    Doctor doc = CreateDoctorOrSurgeon(currentId++, spec, makeSurgeon);
                    doctors.Add(doc);
                }
                if (currentId > count) break;
            }

            // Fill remaining count with random specializations
            while (currentId <= count)
            {
                string spec = allSpecializations[random.Next(allSpecializations.Count)];
                bool isSurgicalSpec = surgicalSpecializations.Contains(spec);
                bool makeSurgeon = isSurgicalSpec && random.Next(100) < 25; // Lower chance for random specs
                Doctor doc = CreateDoctorOrSurgeon(currentId++, spec, makeSurgeon);
                doctors.Add(doc);
            }

            Console.WriteLine($"Generated {doctors.Count} doctors/surgeons.");
            return doctors;
        }

        // Helper to create individual doctor/surgeon
        private Doctor CreateDoctorOrSurgeon(int id, string specialization, bool isSurgeon)
        {
            int maxWorkload = random.Next(5, 16);
            ExperienceLevel experienceLevel = (ExperienceLevel)random.Next(1, 4);

            if (isSurgeon)
            {
                return new Surgeon
                {
                    Id = id,
                    Name = $"Dr. {firstNames[random.Next(firstNames.Count)]} {lastNames[random.Next(lastNames.Count)]} (Surgeon)",
                    Specialization = specialization,
                    Workload = 0,
                    MaxWorkload = maxWorkload,
                    ExperienceLevel = experienceLevel,
                    Preferences = GenerateDoctorPreferences(id, conditions), // Pass conditions list
                    IsAvailableForSurgery = random.Next(100) < 85,
                    Availability = GenerateRealisticAvailabilitySlots() // Use realistic slots
                };
            }
            else
            {
                return new Doctor
                {
                    Id = id,
                    Name = $"Dr. {firstNames[random.Next(firstNames.Count)]} {lastNames[random.Next(lastNames.Count)]}",
                    Specialization = specialization,
                    Workload = 0,
                    MaxWorkload = maxWorkload,
                    ExperienceLevel = experienceLevel,
                    Preferences = GenerateDoctorPreferences(id, conditions) // Pass conditions list
                };
            }
        }

        /// <summary>
        /// Generates Operating Rooms, ensuring some match surgical specializations.
        /// </summary>
        public List<OperatingRoom> GenerateOperatingRooms(int count, List<MedicalProcedure> procedures)
        {
            List<OperatingRoom> operatingRooms = new List<OperatingRoom>();
            var requiredSurgicalSpecs = procedures.Where(p => p.IsOperation).Select(p => p.RequiredSpecialization).Distinct().ToList();
            int currentId = 1;

            // Create at least one OR per required surgical specialization
            foreach (string spec in requiredSurgicalSpecs)
            {
                if (currentId > count) break;
                operatingRooms.Add(CreateOperatingRoom(currentId++, true, spec));
            }

            // Fill the rest with general or randomly specialized rooms
            while (currentId <= count)
            {
                bool isSpecialized = random.Next(100) < 30; // Lower chance for remaining ORs
                string spec = isSpecialized ? surgicalSpecializations[random.Next(surgicalSpecializations.Count)] : null;
                operatingRooms.Add(CreateOperatingRoom(currentId++, isSpecialized, spec));
            }
            Console.WriteLine($"Generated {operatingRooms.Count} operating rooms.");
            return operatingRooms;
        }

        // Helper to create an operating room
        private OperatingRoom CreateOperatingRoom(int id, bool isSpecialized, string specialization)
        {
            // Start with basic equipment
            List<string> equipment = new List<string>(basicOrEquipment);
            // Add specialized equipment
            if (isSpecialized && !string.IsNullOrEmpty(specialization) && equipmentBySpecialization.TryGetValue(specialization, out var specEquip))
            {
                equipment.AddRange(specEquip);
            }
            // Add a few extra random pieces
            equipment.AddRange(GenerateRandomEquipment(random.Next(1, 4)));

            return new OperatingRoom
            {
                Id = id,
                Name = $"OR-{id:D2}" + (isSpecialized ? $" ({specialization})" : " (General)"),
                IsSpecialized = isSpecialized,
                Specialization = specialization,
                AvailableEquipment = equipment.Distinct().ToList(), // Ensure unique
                AvailabilityHours = GenerateRealisticORAvailability(), // Use realistic TimeRange
                ScheduledSlots = new List<TimeSlot>()
            };
        }

        /// <summary>
        /// Generates Patients, linking them to existing specializations and procedures.
        /// </summary>
        public List<Patient> GeneratePatients(int count, List<Doctor> doctors, List<MedicalProcedure> procedures)
        {
            List<Patient> patients = new List<Patient>();
            var availableSpecializations = doctors.Select(d => d.Specialization).Distinct().ToList();
            var proceduresLookup = procedures.ToLookup(p => p.RequiredSpecialization); // Group procedures by spec

            for (int i = 1; i <= count; i++)
            {
                // Assign specialization based on available doctors
                string specialization = availableSpecializations[random.Next(availableSpecializations.Count)];
                string condition = conditions[random.Next(conditions.Count)]; // Could correlate condition to spec later
                UrgencyLevel urgency = (UrgencyLevel)random.Next(1, 4);
                ComplexityLevel complexity = (ComplexityLevel)random.Next(1, 4);
                bool needsSurgery = surgicalSpecializations.Contains(specialization) && random.Next(100) < 40; // 40% chance if surgical spec

                int? requiredProcedureId = null;
                if (needsSurgery && proceduresLookup.Contains(specialization) && proceduresLookup[specialization].Any(p => p.IsOperation))
                {
                    // Select a surgery procedure matching the specialization
                    var possibleProcedures = proceduresLookup[specialization].Where(p => p.IsOperation).ToList();
                    requiredProcedureId = possibleProcedures[random.Next(possibleProcedures.Count)].Id;
                }
                else
                {
                    needsSurgery = false; // Can't need surgery if no matching procedure exists or not surgical spec
                }

                Patient patient = new Patient
                {
                    Id = i,
                    Name = $"{firstNames[random.Next(firstNames.Count)]} {lastNames[random.Next(lastNames.Count)]}",
                    Condition = condition,
                    Urgency = urgency,
                    RequiredSpecialization = specialization,
                    NeedsSurgery = needsSurgery,
                    AdmissionDate = DateTime.Now.AddDays(-random.Next(1, 20)), // Admitted in last 20 days
                    ComplexityLevel = complexity,
                    EstimatedTreatmentTime = needsSurgery && requiredProcedureId.HasValue ?
                                             procedures.First(p => p.Id == requiredProcedureId.Value).EstimatedDuration : // Use procedure duration
                                             random.Next(1, 3) + random.NextDouble(), // Shorter time for non-surgery
                    RequiredProcedureId = requiredProcedureId,
                    ScheduledSurgeryDate = null,
                    AssignedDoctorId = null,
                    AssignedSurgeonId = null,
                    AssignedOperatingRoomId = null,
                    PreviousDoctors = new List<int>()
                };

                // Assign previous doctors logically
                if (doctors.Any() && random.Next(100) < 30) // 30% chance
                {
                    var potentialPrevDoctors = doctors.Where(d => d.Specialization == patient.RequiredSpecialization).ToList();
                    if (potentialPrevDoctors.Any())
                    {
                        patient.PreviousDoctors.Add(potentialPrevDoctors[random.Next(potentialPrevDoctors.Count)].Id);
                    }
                }
                patients.Add(patient);
            }
            Console.WriteLine($"Generated {patients.Count} patients.");
            return patients;
        }


        // --- Updated Helper Methods ---

        private List<string> GenerateRandomEquipment(int count)
        {
            return allEquipment.OrderBy(_ => random.Next()).Take(count).ToList();
        }

        // Generates structured preferences for a doctor
        private List<DoctorPreference> GenerateDoctorPreferences(int doctorId, List<string> availableConditions)
        {
            List<DoctorPreference> preferences = new List<DoctorPreference>();
            int preferencesCount = random.Next(0, 3); // 0, 1, or 2 preferences
            var possibleTypes = Enum.GetValues(typeof(PreferenceType)).Cast<PreferenceType>().ToList();

            for (int i = 0; i < preferencesCount; i++)
            {
                var pref = new DoctorPreference { DoctorId = doctorId };
                pref.Type = possibleTypes[random.Next(possibleTypes.Count)];
                pref.Direction = (random.Next(2) == 0) ? PreferenceDirection.Prefers : PreferenceDirection.Avoids;

                switch (pref.Type)
                {
                    case PreferenceType.PatientComplexity:
                        pref.LevelValue = random.Next(1, 4); break;
                    case PreferenceType.PatientUrgency:
                        pref.LevelValue = random.Next(1, 4); break;
                    case PreferenceType.PatientCondition:
                        if (availableConditions.Any()) { pref.ConditionValue = availableConditions[random.Next(availableConditions.Count)]; pref.LevelValue = null; }
                        else continue; // Skip if no conditions
                        break;
                }
                if (!preferences.Any(p => p.Type == pref.Type)) preferences.Add(pref); // Avoid duplicate types
            }
            return preferences;
        }

        // Generates more realistic availability slots for surgeons
        private List<AvailabilitySlot> GenerateRealisticAvailabilitySlots()
        {
            var availability = new List<AvailabilitySlot>();
            // More likely Mon-Fri, standard blocks
            for (int day = 1; day <= 5; day++) // Monday to Friday
            {
                if (random.Next(100) < 85) // 85% chance of being available on a weekday
                {
                    // Morning Block (8/9 AM to 12/1 PM)
                    if (random.Next(100) < 80)
                    {
                        availability.Add(new AvailabilitySlot
                        {
                            DayOfWeek = (DayOfWeek)day,
                            StartTime = TimeSpan.FromHours(8 + random.Next(0, 2)),
                            EndTime = TimeSpan.FromHours(12 + random.Next(0, 2))
                        });
                    }
                    // Afternoon Block (1/2 PM to 5/6 PM)
                    if (random.Next(100) < 80)
                    {
                        availability.Add(new AvailabilitySlot
                        {
                            DayOfWeek = (DayOfWeek)day,
                            StartTime = TimeSpan.FromHours(13 + random.Next(0, 2)),
                            EndTime = TimeSpan.FromHours(17 + random.Next(0, 2))
                        });
                    }
                }
            }
            // Optional: Add rare weekend availability
            return availability;
        }

        // Generates realistic availability hours for an OR using TimeRange
        private Dictionary<DayOfWeek, List<TimeRange>> GenerateRealisticORAvailability()
        {
            var availability = new Dictionary<DayOfWeek, List<TimeRange>>();
            for (int day = 1; day <= 5; day++) // Monday to Friday
            {
                var ranges = new List<TimeRange>();
                // High chance of standard blocks
                if (random.Next(100) < 95) ranges.Add(new TimeRange { StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) });
                if (random.Next(100) < 95) ranges.Add(new TimeRange { StartTime = TimeSpan.FromHours(13), EndTime = TimeSpan.FromHours(17) });
                // Maybe an evening block sometimes?
                if (random.Next(100) < 15) ranges.Add(new TimeRange { StartTime = TimeSpan.FromHours(18), EndTime = TimeSpan.FromHours(21) });

                if (ranges.Any()) availability[(DayOfWeek)day] = ranges;
            }
            return availability;
        }

        // Removed GenerateInitialSchedule method
    }
}
