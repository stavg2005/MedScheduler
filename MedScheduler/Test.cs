using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedScheduler
{
    internal class Test
    {

            static async void Main1(string[] args)
            {
                var doctors = new List<Doctor>
        {
            new Doctor { Id = 1, Specialization = "Cardiology", MaxWorkload = 10 },
            new Doctor { Id = 2, Specialization = "Orthopedics", MaxWorkload = 8 },
            new Doctor { Id = 3, Specialization = "Neurology", MaxWorkload = 12 }
        };

                var patients = new List<Patient>
        {
            new Patient { Id = 1, Condition = "Heart Disease", Urgency = "High", RequiredSpecialization = "Cardiology" },
            new Patient { Id = 2, Condition = "Broken Leg", Urgency = "Medium", RequiredSpecialization = "Orthopedics" },
            new Patient { Id = 3, Condition = "Migraine", Urgency = "Low", RequiredSpecialization = "Neurology" }
        };

                var genetics = new DoctorScheduler(100, doctors, patients);
                var bestSchedule =  genetics.Solve();

                // Output the best schedule
                foreach (var doctorId in bestSchedule.DoctorToPatients.Keys)
                {
                    Console.WriteLine($"Doctor {doctorId} is assigned to patients: {string.Join(", ", bestSchedule.DoctorToPatients[doctorId])}");
                }
            }
        }
    }
