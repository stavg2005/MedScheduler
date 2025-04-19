using System;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary1;
using Models; // Assuming all models are here

namespace MedScheduler // Or your appropriate namespace
{
    /// <summary>
    /// Generates semi-realistic, correlated test data for scheduling algorithms.
    /// </summary>
    public class DataGenerator
    {
        private Random random = new Random();

        // --- Base Data for Correlation ---

        private List<string> medicalSpecializations = new List<string> {
            "Cardiology", "Neurology", "Pediatrics", "Dermatology", "Oncology",
            "Pulmonology", "Gastroenterology", "Endocrinology", "Rheumatology",
            "Psychiatry", "Nephrology", "Geriatrics", "Infectious Disease", "Hematology"
        };
        private List<string> surgicalSpecializations = new List<string> {
            "General Surgery", "Orthopedics", "Neurosurgery", "Ophthalmology", "Urology", "OB/GYN", "Cardiac Surgery"
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
            { "Cardiology", new List<string> { "Angioplasty", "Stent Placement", "Cardiac Catheterization", "Pacemaker Implantation" } },
            { "Gastroenterology", new List<string> { "Colonoscopy", "Endoscopy (EGD)" } },
            { "Oncology", new List<string> { "Biopsy", "Lumpectomy", "Chemotherapy Administration" } }
            // Add more mappings...
        };

        // Basic lists for names and conditions
        private List<string> firstNames = new List<string> { "John", "Sarah", "Michael", "Emma", "David", "Jennifer", "Robert", "Emily", "William", "Olivia", "James", "Sophia", "Benjamin", "Ava", "Daniel", "Isabella", "Matthew", "Mia", "Joseph", "Charlotte", "Andrew", "Amelia", "Samuel", "Harper", "Anthony", "Abigail", "Christopher", "Elizabeth", "Ryan", "Sofia", "Nathan", "Ella" };
        private List<string> lastNames = new List<string> { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "Hernandez", "King", "Wright", "Lopez" };
        private List<string> conditions = new List<string> { "Heart Disease", "Migraine", "Broken Leg", "Pneumonia", "Eczema", "Lung Cancer", "Chest Pain", "COPD", "Ulcerative Colitis", "Type 1 Diabetes", "Rheumatoid Arthritis", "Cataracts", "Depression", "Kidney Stones", "Chronic Kidney Disease", "Dementia", "HIV", "Anemia", "Pregnancy Complications", "Appendicitis", "Gallstones", "ACL Tear", "Brain Tumor", "Glaucoma", "Benign Prostatic Hyperplasia", "Fibroids", "Arrhythmia" };


        // --- Enhanced Generation Methods ---

        /// <summary>
        /// Generates Procedures first, ensuring they align with defined specializations.
        /// </summary>
        public List<MedicalProcedure> GenerateProcedures(int count)
        {
            List<MedicalProcedure> procedures = new List<MedicalProcedure>();
            List<string> availableSpecs = proceduresBySpecialization.Keys.ToList();
            if (!availableSpecs.Any()) return procedures;

            for (int i = 1; i <= count; i++)
            {
                string specialization = availableSpecs[random.Next(availableSpecs.Count)];
                string procedureName = proceduresBySpecialization[specialization][random.Next(proceduresBySpecialization[specialization].Count)];
                bool isOperation = surgicalSpecializations.Contains(specialization);
                int complexityLevelInt = random.Next(1, 4);
                ExperienceLevel minExperience = (ExperienceLevel)complexityLevelInt; // Simple mapping

                MedicalProcedure procedure = new MedicalProcedure
                {
                    Id = i,
                    Name = procedureName,
                    RequiredSpecialization = specialization,
                    EstimatedDuration = random.Next(1, 5) + Math.Round(random.NextDouble(), 1),
                    IsOperation = isOperation,
                    ComplexityLevel = complexityLevelInt,
                    MinimumDoctorExperienceLevel = minExperience
                    // RequiredEquipment removed
                };
                procedures.Add(procedure);
            }
            Console.WriteLine($"Generated {procedures.Count} procedures.");
            return procedures;
        }

        /// <summary>
        /// Generates Doctors and Surgeons, ensuring coverage for specializations found in procedures.
        /// </summary>
        public List<Doctor> GenerateDoctors(int count, List<MedicalProcedure> procedures)
        {
            List<Doctor> doctors = new List<Doctor>();
            // Ensure we have specs based on procedures provided
            var specsWithProcedures = procedures?.Select(p => p.RequiredSpecialization).Distinct().ToList() ?? new List<string>();
            if (!specsWithProcedures.Any()) specsWithProcedures = allSpecializations; // Fallback if no procedures

            var specsRequiringSurgery = procedures?.Where(p => p.IsOperation).Select(p => p.RequiredSpecialization).Distinct().ToList() ?? new List<string>();
            int currentId = 1;

            // Ensure at least a few doctors per specialization that has procedures
            foreach (string spec in specsWithProcedures)
            {
                int numDoctorsForSpec = random.Next(2, 6);
                for (int i = 0; i < numDoctorsForSpec; i++)
                {
                    if (currentId > count) break;
                    bool isSurgicalSpec = specsRequiringSurgery.Contains(spec);
                    bool makeSurgeon = isSurgicalSpec && random.Next(100) < 90; // Higher chance if surgical spec
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
                bool makeSurgeon = isSurgicalSpec && random.Next(100) < 50;
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
            ExperienceLevel experienceLevel = (ExperienceLevel)random.Next(1, 4); // Cast int 1,2,3 to Enum

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
                    Preferences = GenerateDoctorPreferences(id, conditions),
                    IsAvailableForSurgery = random.Next(100) < 85,
                    Availability = GenerateRealisticAvailabilitySlots()
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
                    Preferences = GenerateDoctorPreferences(id, conditions)
                };
            }
        }

        /// <summary>
        /// Generates generic Operating Rooms.
        /// </summary>
        public List<OperatingRoom> GenerateOperatingRooms(int count /*, List<MedicalProcedure> procedures - removed */)
        {
            List<OperatingRoom> operatingRooms = new List<OperatingRoom>();
            for (int i = 1; i <= count; i++)
            {
                OperatingRoom operatingRoom = new OperatingRoom
                {
                    Id = i,
                    Name = $"OR-{i:D2}", // Simple name
                    // Removed IsSpecialized, Specialization, AvailableEquipment
                    AvailabilityHours = GenerateRealisticORAvailability(),
                    ScheduledSlots = new List<TimeSlot>()
                };
                operatingRooms.Add(operatingRoom);
            }
            Console.WriteLine($"Generated {operatingRooms.Count} generic operating rooms.");
            return operatingRooms;
        }

        /// <summary>
        /// Generates Patients, linking them to existing specializations and procedures.
        /// </summary>
        public List<Patient> GeneratePatients(int count, List<Doctor> doctors, List<MedicalProcedure> procedures)
        {
            List<Patient> patients = new List<Patient>();
            if (doctors == null || !doctors.Any() || procedures == null)
            {
                Console.WriteLine("Warning: Cannot generate patients without doctors and procedures.");
                return patients; // Return empty list if prerequisites missing
            }

            var availableSpecializations = doctors.Select(d => d.Specialization).Distinct().ToList();
            if (!availableSpecializations.Any())
            {
                Console.WriteLine("Warning: No specializations found among doctors. Cannot generate patients accurately.");
                return patients;
            }

            var proceduresLookup = procedures.ToLookup(p => p.RequiredSpecialization);
            var surgeryProcedures = procedures.Where(p => p.IsOperation).ToList();

            for (int i = 1; i <= count; i++)
            {
                string specialization = availableSpecializations[random.Next(availableSpecializations.Count)];
                string condition = conditions[random.Next(conditions.Count)];
                UrgencyLevel urgency = (UrgencyLevel)random.Next(1, 4);
                ComplexityLevel complexity = (ComplexityLevel)random.Next(1, 4);
                // Higher chance of needing surgery if it's a surgical specialization
                bool needsSurgery = surgicalSpecializations.Contains(specialization) && random.Next(100) < 40;

                int? requiredProcedureId = null;
                var possibleProcedures = proceduresLookup.Contains(specialization) ? proceduresLookup[specialization].Where(p => p.IsOperation).ToList() : new List<MedicalProcedure>();

                if (needsSurgery && possibleProcedures.Any())
                {
                    requiredProcedureId = possibleProcedures[random.Next(possibleProcedures.Count)].Id;
                }
                else
                {
                    needsSurgery = false; // Ensure flag is false if no suitable procedure found/assigned
                }

                Patient patient = new Patient
                {
                    Id = i,
                    Name = $"{firstNames[random.Next(firstNames.Count)]} {lastNames[random.Next(lastNames.Count)]}",
                    Condition = condition,
                    Urgency = urgency,
                    RequiredSpecialization = specialization,
                    NeedsSurgery = needsSurgery,
                    AdmissionDate = DateTime.Now.AddDays(-random.Next(1, 20)),
                    ComplexityLevel = complexity,
                    EstimatedTreatmentTime = needsSurgery && requiredProcedureId.HasValue ?
                                             procedures.FirstOrDefault(p => p.Id == requiredProcedureId.Value)?.EstimatedDuration // Use procedure duration safely
                                             : random.Next(1, 3) + random.NextDouble(),
                    RequiredProcedureId = requiredProcedureId,
                    ScheduledSurgeryDate = null,
                    AssignedDoctorId = null,
                    AssignedSurgeonId = null,
                    AssignedOperatingRoomId = null,
                    PreviousDoctors = new List<int>()
                };

                // Assign previous doctors logically
                if (random.Next(100) < 30)
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


        // --- Helper Methods ---

        // Generates structured preferences
        private List<DoctorPreference> GenerateDoctorPreferences(int doctorId, List<string> availableConditions)
        {
            List<DoctorPreference> preferences = new List<DoctorPreference>(); int preferencesCount = random.Next(0, 3);
            var possibleTypes = Enum.GetValues(typeof(PreferenceType)).Cast<PreferenceType>().ToList();
            for (int i = 0; i < preferencesCount; i++)
            {
                var pref = new DoctorPreference { DoctorId = doctorId }; pref.Type = possibleTypes[random.Next(possibleTypes.Count)]; pref.Direction = (random.Next(2) == 0) ? PreferenceDirection.Prefers : PreferenceDirection.Avoids;
                switch (pref.Type) { case PreferenceType.PatientComplexity: pref.LevelValue = random.Next(1, 4); break; case PreferenceType.PatientUrgency: pref.LevelValue = random.Next(1, 4); break; case PreferenceType.PatientCondition: if (availableConditions.Any()) { pref.ConditionValue = availableConditions[random.Next(availableConditions.Count)]; pref.LevelValue = null; } else continue; break; }
                if (!preferences.Any(p => p.Type == pref.Type)) preferences.Add(pref);
            }
            return preferences;
        }

        // Generates realistic availability slots for surgeons
        private List<AvailabilitySlot> GenerateRealisticAvailabilitySlots()
        {
            var availability = new List<AvailabilitySlot>();
            for (int day = 1; day <= 5; day++)
            { // Mon-Fri
                if (random.Next(100) < 85)
                { // Chance to be available on this day
                    if (random.Next(100) < 80) availability.Add(new AvailabilitySlot { DayOfWeek = (DayOfWeek)day, StartTime = TimeSpan.FromHours(8 + random.Next(0, 2)), EndTime = TimeSpan.FromHours(12 + random.Next(0, 2)) });
                    if (random.Next(100) < 80) availability.Add(new AvailabilitySlot { DayOfWeek = (DayOfWeek)day, StartTime = TimeSpan.FromHours(13 + random.Next(0, 2)), EndTime = TimeSpan.FromHours(17 + random.Next(0, 2)) });
                }
            }
            return availability;
        }

        // Generates realistic availability hours for an OR using TimeRange
        private Dictionary<DayOfWeek, List<TimeRange>> GenerateRealisticORAvailability()
        {
            var availability = new Dictionary<DayOfWeek, List<TimeRange>>();
            for (int day = 1; day <= 5; day++)
            { // Mon-Fri
                var ranges = new List<TimeRange>();
                if (random.Next(100) < 95) ranges.Add(new TimeRange { StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) });
                if (random.Next(100) < 95) ranges.Add(new TimeRange { StartTime = TimeSpan.FromHours(13), EndTime = TimeSpan.FromHours(17) });
                if (random.Next(100) < 15) ranges.Add(new TimeRange { StartTime = TimeSpan.FromHours(18), EndTime = TimeSpan.FromHours(21) });
                if (ranges.Any()) availability[(DayOfWeek)day] = ranges;
            }
            return availability;
        }

    }
}
