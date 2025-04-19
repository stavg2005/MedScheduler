using ClassLibrary1;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    /// <summary>
    /// Generates a .sql script file with batch INSERT statements
    /// for populating the MySQL database with generated data.
    /// </summary>
    class SqlScriptGenerator
    {
   

            private int _batchSize = 100; // Number of rows per INSERT statement

            // --- SQL Formatting Helpers ---

            private string EscapeSqlString(string value)
            {
                if (value == null) return "NULL";
                // Basic escaping for MySQL: escape backslash, single quote, double quote, null char, ctrl-z
                return "'" + value.Replace("\\", "\\\\").Replace("'", "''").Replace("\"", "\\\"").Replace("\0", "\\0").Replace("\x1A", "\\Z") + "'";
            }

            private string FormatSqlInt(int value) => value.ToString();
            private string FormatSqlNullableInt(int? value) => value.HasValue ? value.Value.ToString() : "NULL";
            private string FormatSqlDouble(double value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture); // Use invariant culture for decimal point
            private string FormatSqlNullableDouble(double? value) => value.HasValue ? value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "NULL";
            private string FormatSqlBool(bool value) => value ? "1" : "0"; // MySQL uses 1/0 for BOOLEAN/BOOL/TINYINT(1)
            private string FormatSqlNullableBool(bool? value) => value.HasValue ? (value.Value ? "1" : "0") : "NULL";
            private string FormatSqlDateTime(DateTime value) => "'" + value.ToString("yyyy-MM-dd HH:mm:ss") + "'"; // Standard MySQL DATETIME format
            private string FormatSqlNullableDateTime(DateTime? value) => value.HasValue ? "'" + value.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'" : "NULL";
            private string FormatSqlTime(TimeSpan value) => $"'{Math.Min(23, value.Hours):D2}:{value.Minutes:D2}:{value.Seconds:D2}'"; // Format as HH:MM:SS, cap hours if needed for TIME type


            // --- INSERT Statement Generators ---

            public string GenerateDoctorInserts(List<Doctor> doctors)
            {
                if (doctors == null || !doctors.Any()) return "-- No Doctors to insert.\n";
                StringBuilder sb = new StringBuilder();
                string table = "Doctors";
                string cols = "(`Id`, `Name`, `Specialization`, `Workload`, `MaxWorkload`, `ExperienceLevel`, `IsSurgeon`, `IsAvailableForSurgery`)";

                for (int i = 0; i < doctors.Count; i += _batchSize)
                {
                    var batch = doctors.Skip(i).Take(_batchSize);
                    sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                    var values = batch.Select(doc =>
                    {
                        bool isSurgeon = doc is Surgeon;
                        bool? isAvailable = (doc as Surgeon)?.IsAvailableForSurgery; // Null if not a surgeon
                        return $"({FormatSqlInt(doc.Id)}, {EscapeSqlString(doc.Name)}, {EscapeSqlString(doc.Specialization)}, {FormatSqlInt(doc.Workload)}, {FormatSqlInt(doc.MaxWorkload)}, {(int)doc.ExperienceLevel}, {FormatSqlBool(isSurgeon)}, {FormatSqlNullableBool(isAvailable)})";
                    });
                    sb.AppendLine(string.Join(",\n", values) + ";");
                }
                return sb.ToString();
            }

            public string GenerateProcedureInserts(List<MedicalProcedure> procedures)
            {
                if (procedures == null || !procedures.Any()) return "-- No Procedures to insert.\n";
                StringBuilder sb = new StringBuilder();
                string table = "MedicalProcedures"; // Assuming table name
                string cols = "(`Id`, `Name`, `RequiredSpecialization`, `EstimatedDuration`, `IsOperation`, `ComplexityLevel`, `MinimumDoctorExperienceLevel`)"; // Base columns

                for (int i = 0; i < procedures.Count; i += _batchSize)
                {
                    var batch = procedures.Skip(i).Take(_batchSize);
                    sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                    var values = batch.Select(proc =>
                        $"({FormatSqlInt(proc.Id)}, {EscapeSqlString(proc.Name)}, {EscapeSqlString(proc.RequiredSpecialization)}, {FormatSqlDouble(proc.EstimatedDuration)}, {FormatSqlBool(proc.IsOperation)}, {FormatSqlInt(proc.ComplexityLevel)}, {(int)proc.MinimumDoctorExperienceLevel})"
                    );
                    sb.AppendLine(string.Join(",\n", values) + ";");
                }
                // Note: RequiredEquipment needs a separate linking table and inserts
                return sb.ToString();
            }

            public string GenerateOperatingRoomInserts(List<OperatingRoom> rooms)
            {
                if (rooms == null || !rooms.Any()) return "-- No Operating Rooms to insert.\n";
                StringBuilder sb = new StringBuilder();
                string table = "OperatingRooms";
                // *** Simplified Columns ***
                string cols = "(`Id`, `Name`)";

                for (int i = 0; i < rooms.Count; i += _batchSize)
                {
                    var batch = rooms.Skip(i).Take(_batchSize);
                    sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                    var values = batch.Select(room =>
                    // *** Only insert ID and Name ***
                    $"({FormatSqlInt(room.Id)}, {EscapeSqlString(room.Name)})"
                );
                sb.AppendLine(string.Join(",\n", values) + ";");
                }
                // Note: AvailabilityHours needs a separate linking table and inserts
                 return sb.ToString();
        }

            public string GeneratePatientInserts(List<Patient> patients)
            {
                if (patients == null || !patients.Any()) return "-- No Patients to insert.\n";
                StringBuilder sb = new StringBuilder();
                string table = "Patients";
                string cols = "(`Id`, `Name`, `Condition`, `Urgency`, `RequiredSpecialization`, `NeedsSurgery`, `AdmissionDate`, `ScheduledSurgeryDate`, `AssignedDoctorId`, `AssignedSurgeonId`, `ComplexityLevel`, `EstimatedTreatmentTime`, `RequiredProcedureId`, `AssignedOperatingRoomId`)"; // Match your schema

                for (int i = 0; i < patients.Count; i += _batchSize)
                {
                    var batch = patients.Skip(i).Take(_batchSize);
                    sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                    var values = batch.Select(p =>
                        $"({FormatSqlInt(p.Id)}, {EscapeSqlString(p.Name)}, {EscapeSqlString(p.Condition)}, {(int)p.Urgency}, {EscapeSqlString(p.RequiredSpecialization)}, {FormatSqlBool(p.NeedsSurgery)}, {FormatSqlDateTime(p.AdmissionDate)}, {FormatSqlNullableDateTime(p.ScheduledSurgeryDate)}, {FormatSqlNullableInt(p.AssignedDoctorId)}, {FormatSqlNullableInt(p.AssignedSurgeonId)}, {(int)p.ComplexityLevel}, {FormatSqlNullableDouble(p.EstimatedTreatmentTime)}, {FormatSqlNullableInt(p.RequiredProcedureId)}, {FormatSqlNullableInt(p.AssignedOperatingRoomId)})"
                    );
                    sb.AppendLine(string.Join(",\n", values) + ";");
                }
                // Note: PreviousDoctors needs a separate linking table (PatientDoctorHistory) and inserts
                return sb.ToString();
            }

            // --- Inserts for Linking Tables ---

            public string GenerateDoctorPreferenceInserts(List<Doctor> doctors)
            {
                if (doctors == null) return "-- No Doctors for preferences.\n";
                StringBuilder sb = new StringBuilder();
                string table = "DoctorPreferences"; // Assuming table name
                string cols = "(`DoctorId`, `PreferenceType`, `PreferenceDirection`, `LevelValue`, `ConditionValue`)";
                var allPrefs = new List<string>();

                foreach (var doctor in doctors)
                {
                    if (doctor.Preferences != null && doctor.Preferences.Any())
                    {
                        foreach (var pref in doctor.Preferences)
                        {
                            allPrefs.Add($"({FormatSqlInt(doctor.Id)}, {EscapeSqlString(pref.Type.ToString())}, {EscapeSqlString(pref.Direction.ToString())}, {FormatSqlNullableInt(pref.LevelValue)}, {EscapeSqlString(pref.ConditionValue)})");
                        }
                    }
                }

                if (!allPrefs.Any()) return "-- No Doctor Preferences to insert.\n";

                for (int i = 0; i < allPrefs.Count; i += _batchSize)
                {
                    var batchValues = allPrefs.Skip(i).Take(_batchSize);
                    sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                    sb.AppendLine(string.Join(",\n", batchValues) + ";");
                }
                return sb.ToString();
            }

            public string GenerateSurgeonAvailabilityInserts(List<Doctor> doctors)
            {
                if (doctors == null) return "-- No Doctors for availability.\n";
                StringBuilder sb = new StringBuilder();
                string table = "SurgeonAvailabilitySlots"; // Assuming table name
                string cols = "(`SurgeonId`, `DayOfWeek`, `StartTime`, `EndTime`)";
                var allSlots = new List<string>();

                foreach (var doctor in doctors)
                {
                    if (doctor is Surgeon surgeon && surgeon.Availability != null && surgeon.Availability.Any())
                    {
                        foreach (var slot in surgeon.Availability)
                        {
                            // Map C# DayOfWeek (Sun=0) to potential SQL convention if needed, or store int directly
                            int dayOfWeekInt = (int)slot.DayOfWeek;
                            allSlots.Add($"({FormatSqlInt(surgeon.Id)}, {dayOfWeekInt}, {FormatSqlTime(slot.StartTime)}, {FormatSqlTime(slot.EndTime)})");
                        }
                    }
                }

                if (!allSlots.Any()) return "-- No Surgeon Availability Slots to insert.\n";

                for (int i = 0; i < allSlots.Count; i += _batchSize)
                {
                    var batchValues = allSlots.Skip(i).Take(_batchSize);
                    sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                    sb.AppendLine(string.Join(",\n", batchValues) + ";");
                }
                return sb.ToString();
            }

            public string GeneratePatientDoctorHistoryInserts(List<Patient> patients)
            {
                if (patients == null) return "-- No Patients for history.\n";
                StringBuilder sb = new StringBuilder();
                string table = "PatientDoctorHistory"; // Assuming table name
                string cols = "(`PatientId`, `DoctorId`)"; // Assuming basic history table
                var allHistory = new List<string>();

                foreach (var patient in patients)
                {
                    if (patient.PreviousDoctors != null && patient.PreviousDoctors.Any())
                    {
                        foreach (var doctorId in patient.PreviousDoctors)
                        {
                            // Ensure doctorId actually exists? Optional check.
                            allHistory.Add($"({FormatSqlInt(patient.Id)}, {FormatSqlInt(doctorId)})");
                        }
                    }
                }

                if (!allHistory.Any()) return "-- No Patient Doctor History to insert.\n";

                for (int i = 0; i < allHistory.Count; i += _batchSize)
                {
                    var batchValues = allHistory.Skip(i).Take(_batchSize);
                    sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                    sb.AppendLine(string.Join(",\n", batchValues) + ";");
                }
                return sb.ToString();
            }

        // *** NEW: Generate OR Availability Inserts ***
        public string GenerateOperatingRoomAvailabilityInserts(List<OperatingRoom> rooms)
        {
            if (rooms == null) return "-- No Operating Rooms for availability.\n";
            StringBuilder sb = new StringBuilder();
            string table = "OperatingRoomAvailability";
            string cols = "(`OperatingRoomId`, `DayOfWeek`, `StartTime`, `EndTime`)";
            var allSlots = new List<string>();

            foreach (var room in rooms)
            {
                if (room.AvailabilityHours != null)
                {
                    foreach (var kvp in room.AvailabilityHours) // kvp.Key is DayOfWeek, kvp.Value is List<TimeRange>
                    {
                        DayOfWeek day = kvp.Key;
                        List<TimeRange> ranges = kvp.Value;
                        if (ranges != null)
                        {
                            foreach (var timeRange in ranges)
                            {
                                allSlots.Add($"({FormatSqlInt(room.Id)}, {(int)day}, {FormatSqlTime(timeRange.StartTime)}, {FormatSqlTime(timeRange.EndTime)})");
                            }
                        }
                    }
                }
            }

            if (!allSlots.Any()) return "-- No Operating Room Availability to insert.\n";

            for (int i = 0; i < allSlots.Count; i += _batchSize)
            {
                var batchValues = allSlots.Skip(i).Take(_batchSize);
                sb.AppendLine($"INSERT INTO {table} {cols} VALUES");
                sb.AppendLine(string.Join(",\n", batchValues) + ";");
            }
            return sb.ToString();
        }


        /// <summary>
        /// Generates a full SQL script file with batch inserts for all data.
        /// </summary>
        public void GenerateFullScript(string filePath, List<Doctor> doctors, List<Patient> patients, List<MedicalProcedure> procedures, List<OperatingRoom> rooms)
            {
                Console.WriteLine($"Generating SQL script at: {filePath}");
                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine("-- MedScheduler Data Generation Script");
                        writer.WriteLine($"-- Generated on: {DateTime.Now}");
                        writer.WriteLine("-- Ensure tables are created and empty before running.");
                        writer.WriteLine("-- Disable foreign key checks for bulk insert");
                        writer.WriteLine("SET FOREIGN_KEY_CHECKS=0;");
                        writer.WriteLine();

                        // IMPORTANT: INSERT IN ORDER OF DEPENDENCY
                        writer.WriteLine("-- Inserting Doctors --");
                        writer.Write(GenerateDoctorInserts(doctors));
                        writer.WriteLine();

                        writer.WriteLine("-- Inserting Medical Procedures --");
                        writer.Write(GenerateProcedureInserts(procedures));
                        // TODO: Insert into ProcedureEquipment linking table here
                        writer.WriteLine();

                        writer.WriteLine("-- Inserting Operating Rooms --");
                        writer.Write(GenerateOperatingRoomInserts(rooms));
                        // TODO: Insert into OperatingRoomEquipment linking table here
                        // TODO: Insert into OperatingRoomAvailability linking table here
                        writer.WriteLine();

                        writer.WriteLine("-- Inserting Patients --");
                        writer.Write(GeneratePatientInserts(patients));
                        writer.WriteLine();

                        // Insert into linking tables last
                        writer.WriteLine("-- Inserting Doctor Preferences --");
                        writer.Write(GenerateDoctorPreferenceInserts(doctors));
                        writer.WriteLine();

                        writer.WriteLine("-- Inserting Surgeon Availability --");
                        writer.Write(GenerateSurgeonAvailabilityInserts(doctors));
                        writer.WriteLine();

                        writer.WriteLine("-- Inserting Patient Doctor History --");
                        writer.Write(GeneratePatientDoctorHistoryInserts(patients));
                        writer.WriteLine();


                        writer.WriteLine("-- Re-enable foreign key checks --");
                        writer.WriteLine("SET FOREIGN_KEY_CHECKS=1;");
                        writer.WriteLine();
                        writer.WriteLine("-- Script generation complete --");
                    }
                    Console.WriteLine("SQL script generation finished successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating SQL script: {ex.Message}");
                }
            }
        }
    }
