using ClassLibrary1;
using Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes the DatabaseManager with the MySQL connection string.
        /// </summary>
        /// <param name="connectionString">MySQL connection string.</param>
        public DatabaseManager(string connectionString)
        {
            // Connection string should be passed in, read from config/secrets
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // --- Safe Data Reading Helpers ---
        // (These help prevent errors if a database value is NULL)
        private string GetStringSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetString(columnName) : null;
        private int GetInt32Safe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetInt32(columnName) : 0;
        private int? GetNullableInt32Safe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetInt32(columnName) : (int?)null;
        private bool GetBooleanSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetBoolean(columnName) : false;
        private bool? GetNullableBooleanSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetBoolean(columnName) : (bool?)null;
        private double GetDoubleSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetDouble(columnName) : 0.0;
        private double? GetNullableDoubleSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetDouble(columnName) : (double?)null;
        private decimal GetDecimalSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetDecimal(columnName) : 0.0m;
        private decimal? GetNullableDecimalSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetDecimal(columnName) : (decimal?)null;
        private DateTime GetDateTimeSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetDateTime(columnName) : default(DateTime);
        private DateTime? GetNullableDateTimeSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetDateTime(columnName) : (DateTime?)null;
        private TimeSpan GetTimeSpanSafe(MySqlDataReader reader, string columnName) => reader[columnName] != DBNull.Value ? reader.GetTimeSpan(columnName) : default(TimeSpan);

        // Helper to safely cast int to Enum
        private TEnum GetEnumSafe<TEnum>(MySqlDataReader reader, string columnName, TEnum defaultValue) where TEnum : struct, Enum
        {
            int intValue = GetInt32Safe(reader, columnName);
            return Enum.IsDefined(typeof(TEnum), intValue) ? (TEnum)Enum.ToObject(typeof(TEnum), intValue) : defaultValue;
        }


        // --- Main Data Retrieval Methods ---

        /// <summary>
        /// Retrieves all Doctors and Surgeons, including their Preferences and Availability.
        /// </summary>
        public List<Doctor> GetAllDoctorsWithDetails()
        {
            var doctors = new List<Doctor>();
            var doctorLookup = new Dictionary<int, Doctor>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // 1. Fetch base Doctor/Surgeon info
                    string query = "SELECT Id, Name, Specialization, Workload, MaxWorkload, ExperienceLevel, IsSurgeon, IsAvailableForSurgery FROM Doctors;";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bool isSurgeon = GetBooleanSafe(reader, "IsSurgeon");
                            Doctor doctor;

                            if (isSurgeon)
                            {
                                doctor = new Surgeon
                                {
                                    IsAvailableForSurgery = GetNullableBooleanSafe(reader, "IsAvailableForSurgery") ?? false,
                                    Availability = new List<AvailabilitySlot>() // Init list
                                };
                            }
                            else
                            {
                                doctor = new Doctor();
                            }

                            doctor.Id = GetInt32Safe(reader, "Id");
                            doctor.Name = GetStringSafe(reader, "Name");
                            doctor.Specialization = GetStringSafe(reader, "Specialization");
                            doctor.Workload = GetInt32Safe(reader, "Workload");
                            doctor.MaxWorkload = GetInt32Safe(reader, "MaxWorkload");
                            doctor.ExperienceLevel = GetEnumSafe(reader, "ExperienceLevel", ExperienceLevel.Junior);
                            doctor.Preferences = new List<DoctorPreference>(); // Init list
                            doctor.PreviousPatients = new List<int>(); // Init list (will be populated by patient fetch)

                            doctors.Add(doctor);
                            doctorLookup[doctor.Id] = doctor;
                        }
                    } // Reader disposed

                    // 2. Fetch related data if any doctors were found
                    if (doctorLookup.Any())
                    {
                        FetchDoctorPreferences(connection, doctorLookup);
                        FetchSurgeonAvailability(connection, doctorLookup);
                        // Note: PreviousPatients list on Doctor is usually populated when fetching Patients
                    }
                }
                catch (MySqlException ex) { Console.WriteLine($"MySQL Error fetching doctors: {ex.Message}"); }
                catch (Exception ex) { Console.WriteLine($"Error fetching doctors: {ex.Message}"); }
            }
            Console.WriteLine($"Retrieved {doctors.Count} doctors from database.");
            return doctors;
        }

        /// <summary>
        /// Retrieves all Patients, including their previous doctor history.
        /// </summary>
        public List<Patient> GetAllPatientsWithDetails()
        {
            var patients = new List<Patient>();
            var patientLookup = new Dictionary<int, Patient>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // 1. Fetch base Patient info
                    // Use backticks `Condition` if it's a reserved keyword
                    string query = "SELECT Id, Name, `Condition`, Urgency, RequiredSpecialization, NeedsSurgery, AdmissionDate, ScheduledSurgeryDate, AssignedDoctorId, AssignedSurgeonId, ComplexityLevel, EstimatedTreatmentTime, RequiredProcedureId, AssignedOperatingRoomId FROM Patients;";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var p = new Patient();
                            p.Id = GetInt32Safe(reader, "Id");
                            p.Name = GetStringSafe(reader, "Name");
                            p.Condition = GetStringSafe(reader, "Condition");
                            p.Urgency = GetEnumSafe(reader, "Urgency", UrgencyLevel.Low);
                            p.RequiredSpecialization = GetStringSafe(reader, "RequiredSpecialization");
                            p.NeedsSurgery = GetBooleanSafe(reader, "NeedsSurgery");
                            p.AdmissionDate = GetDateTimeSafe(reader, "AdmissionDate");
                            p.ScheduledSurgeryDate = GetNullableDateTimeSafe(reader, "ScheduledSurgeryDate");
                            p.AssignedDoctorId = GetNullableInt32Safe(reader, "AssignedDoctorId");
                            p.AssignedSurgeonId = GetNullableInt32Safe(reader, "AssignedSurgeonId");
                            p.ComplexityLevel = GetEnumSafe(reader, "ComplexityLevel", ComplexityLevel.Simple);
                            // Handle potential conversion from DECIMAL in DB to double? in C#
                            p.EstimatedTreatmentTime = reader["EstimatedTreatmentTime"] != DBNull.Value ? Convert.ToDouble(reader["EstimatedTreatmentTime"]) : (double?)null;
                            p.RequiredProcedureId = GetNullableInt32Safe(reader, "RequiredProcedureId");
                            p.AssignedOperatingRoomId = GetNullableInt32Safe(reader, "AssignedOperatingRoomId");
                            p.PreviousDoctors = new List<int>(); // Initialize

                            patients.Add(p);
                            patientLookup[p.Id] = p;
                        }
                    } // Reader disposed

                    // 2. Fetch Patient History if any patients were found
                    if (patientLookup.Any())
                    {
                        FetchPatientDoctorHistory(connection, patientLookup);
                    }
                }
                catch (MySqlException ex) { Console.WriteLine($"MySQL Error fetching patients: {ex.Message}"); }
                catch (Exception ex) { Console.WriteLine($"Error fetching patients: {ex.Message}"); }
            }
            Console.WriteLine($"Retrieved {patients.Count} patients from database.");
            return patients;
        }

        /// <summary>
        /// Retrieves all Medical Procedures.
        /// </summary>
        public List<MedicalProcedure> GetAllProcedures()
        {
            var procedures = new List<MedicalProcedure>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Id, Name, RequiredSpecialization, EstimatedDuration, IsOperation, ComplexityLevel, MinimumDoctorExperienceLevel FROM MedicalProcedures;";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var proc = new MedicalProcedure();
                            proc.Id = GetInt32Safe(reader, "Id");
                            proc.Name = GetStringSafe(reader, "Name");
                            proc.RequiredSpecialization = GetStringSafe(reader, "RequiredSpecialization");
                            proc.EstimatedDuration = Convert.ToDouble(GetDecimalSafe(reader, "EstimatedDuration")); // Convert Decimal to Double
                            proc.IsOperation = GetBooleanSafe(reader, "IsOperation");
                            proc.ComplexityLevel = GetInt32Safe(reader, "ComplexityLevel"); // Store as int
                            proc.MinimumDoctorExperienceLevel = GetEnumSafe(reader, "MinimumDoctorExperienceLevel", ExperienceLevel.Junior);
                            // proc.RequiredEquipment = new List<string>(); // Fetch separately if needed

                            procedures.Add(proc);
                        }
                    }
                    // TODO: Add separate query to fetch RequiredEquipment if needed
                }
                catch (MySqlException ex) { Console.WriteLine($"MySQL Error fetching procedures: {ex.Message}"); }
                catch (Exception ex) { Console.WriteLine($"Error fetching procedures: {ex.Message}"); }
            }
            Console.WriteLine($"Retrieved {procedures.Count} procedures from database.");
            return procedures;
        }

        /// <summary>
        /// Retrieves all Operating Rooms, including their weekly availability patterns.
        /// </summary>
        public List<OperatingRoom> GetAllOperatingRoomsWithDetails()
        {
            var rooms = new List<OperatingRoom>();
            var roomLookup = new Dictionary<int, OperatingRoom>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    // 1. Fetch base OR info
                    string query = "SELECT Id, Name FROM OperatingRooms;"; // Simplified table
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var room = new OperatingRoom();
                            room.Id = GetInt32Safe(reader, "Id");
                            room.Name = GetStringSafe(reader, "Name");
                            // Properties removed in simplification: IsSpecialized, Specialization, AvailableEquipment
                            room.AvailabilityHours = new Dictionary<DayOfWeek, List<TimeRange>>(); // Init dict
                            room.ScheduledSlots = new List<TimeSlot>(); // Init list

                            rooms.Add(room);
                            roomLookup[room.Id] = room;
                        }
                    } // Reader disposed

                    // 2. Fetch related data if any rooms were found
                    if (roomLookup.Any())
                    {
                        FetchOperatingRoomAvailability(connection, roomLookup);
                        // TODO: Fetch OperatingRoomEquipment if that table exists
                    }
                }
                catch (MySqlException ex) { Console.WriteLine($"MySQL Error fetching ORs: {ex.Message}"); }
                catch (Exception ex) { Console.WriteLine($"Error fetching ORs: {ex.Message}"); }
            }
            Console.WriteLine($"Retrieved {rooms.Count} operating rooms from database.");
            return rooms;
        }


        // --- Helper Methods to Fetch Related Data ---

        private void FetchDoctorPreferences(MySqlConnection connection, Dictionary<int, Doctor> doctorLookup)
        {
            if (!doctorLookup.Any()) return;
            // Ensure connection is open (or handle opening/closing)
            if (connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Warning: Connection closed before fetching preferences."); return;
            }
            string query = "SELECT DoctorId, PreferenceType, PreferenceDirection, LevelValue, ConditionValue FROM DoctorPreferences WHERE DoctorId IN (" + string.Join(",", doctorLookup.Keys) + ");";
            try
            {
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int doctorId = GetInt32Safe(reader, "DoctorId");
                        if (doctorLookup.TryGetValue(doctorId, out Doctor doctor))
                        {
                            var pref = new DoctorPreference { DoctorId = doctorId };
                            string typeStr = GetStringSafe(reader, "PreferenceType");
                            string dirStr = GetStringSafe(reader, "PreferenceDirection");
                            if (Enum.TryParse<PreferenceType>(typeStr, true, out var pType)) pref.Type = pType; else continue;
                            if (Enum.TryParse<PreferenceDirection>(dirStr, true, out var pDir)) pref.Direction = pDir; else continue;
                            pref.LevelValue = GetNullableInt32Safe(reader, "LevelValue");
                            pref.ConditionValue = GetStringSafe(reader, "ConditionValue");
                            doctor.Preferences.Add(pref);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching preferences: {ex.Message}"); }
        }

        private void FetchSurgeonAvailability(MySqlConnection connection, Dictionary<int, Doctor> doctorLookup)
        {
            var surgeonIds = doctorLookup.Values.OfType<Surgeon>().Select(s => s.Id).ToList();
            if (!surgeonIds.Any()) return;
            if (connection.State != System.Data.ConnectionState.Open) { Console.WriteLine("Warning: Connection closed before fetching surgeon availability."); return; }

            string query = "SELECT SurgeonId, DayOfWeek, StartTime, EndTime FROM SurgeonAvailabilitySlots WHERE SurgeonId IN (" + string.Join(",", surgeonIds) + ");";
            try
            {
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int surgeonId = GetInt32Safe(reader, "SurgeonId");
                        if (doctorLookup.TryGetValue(surgeonId, out Doctor doc) && doc is Surgeon surgeon)
                        {
                            var slot = new AvailabilitySlot();
                            int dayInt = GetInt32Safe(reader, "DayOfWeek");
                            if (Enum.IsDefined(typeof(DayOfWeek), dayInt)) slot.DayOfWeek = (DayOfWeek)dayInt; else continue;
                            slot.StartTime = GetTimeSpanSafe(reader, "StartTime");
                            slot.EndTime = GetTimeSpanSafe(reader, "EndTime");
                            surgeon.Availability.Add(slot);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching surgeon availability: {ex.Message}"); }
        }

        private void FetchPatientDoctorHistory(MySqlConnection connection, Dictionary<int, Patient> patientLookup)
        {
            if (!patientLookup.Any()) return;
            if (connection.State != System.Data.ConnectionState.Open) { Console.WriteLine("Warning: Connection closed before fetching patient history."); return; }

            string query = "SELECT PatientId, DoctorId FROM PatientDoctorHistory WHERE PatientId IN (" + string.Join(",", patientLookup.Keys) + ");";
            try
            {
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int patientId = GetInt32Safe(reader, "PatientId");
                        int doctorId = GetInt32Safe(reader, "DoctorId");
                        if (patientLookup.TryGetValue(patientId, out Patient patient))
                        {
                            patient.PreviousDoctors.Add(doctorId);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching patient history: {ex.Message}"); }
        }

        private void FetchOperatingRoomAvailability(MySqlConnection connection, Dictionary<int, OperatingRoom> roomLookup)
        {
            if (!roomLookup.Any()) return;
            if (connection.State != System.Data.ConnectionState.Open) { Console.WriteLine("Warning: Connection closed before fetching OR availability."); return; }

            string query = "SELECT OperatingRoomId, DayOfWeek, StartTime, EndTime FROM OperatingRoomAvailability WHERE OperatingRoomId IN (" + string.Join(",", roomLookup.Keys) + ");";
            try
            {
                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int roomId = GetInt32Safe(reader, "OperatingRoomId");
                        if (roomLookup.TryGetValue(roomId, out OperatingRoom room))
                        {
                            int dayInt = GetInt32Safe(reader, "DayOfWeek");
                            if (!Enum.IsDefined(typeof(DayOfWeek), dayInt)) continue; // Skip invalid day
                            DayOfWeek day = (DayOfWeek)dayInt;

                            TimeRange range = new TimeRange
                            {
                                StartTime = GetTimeSpanSafe(reader, "StartTime"),
                                EndTime = GetTimeSpanSafe(reader, "EndTime")
                            };

                            if (!room.AvailabilityHours.ContainsKey(day))
                            {
                                room.AvailabilityHours[day] = new List<TimeRange>();
                            }
                            room.AvailabilityHours[day].Add(range);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error fetching OR availability: {ex.Message}"); }
        }

    }
}
