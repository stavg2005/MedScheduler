using MedScheduler;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    public class DataSingelton
    {
        private static DataSingelton _instance;
        private static readonly object _lock = new object(); // Thread safety

        // Lists to store shared data
        public List<Doctor> Doctors { get; private set; }
        public List<Patient> Patients { get; private set; }
        public List<OperatingRoom> OperatingRooms { get; private set; }

        // Private constructor to prevent instantiation from outside
        private DataSingelton()
        {
            Doctors = new List<Doctor>();
            Patients = new List<Patient>();
            OperatingRooms = new List<OperatingRoom>();
        }

        // Public property to get the single instance
        public static DataSingelton Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new DataSingelton();
                    return _instance;
                }
            }
        }

        // Method to load data from the database (call this once at startup)
        public void LoadDataFromDatabase(DataManager db)
        {
            Doctors = db.GetDoctors();
            Patients = db.GetPatients();
            OperatingRooms = db.GetOperatingRooms();
        }
    }

}
