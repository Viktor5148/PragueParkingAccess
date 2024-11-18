using System;

namespace PragueParkingAccess
{
    public abstract class Vehicle
    {
        public string RegistrationNumber { get; set; }
        public string VehicleType { get; set; }
        public int Size { get; set; }
        public DateTime ParkingTime { get; set; }

        protected Vehicle(string registrationNumber, string vehicleType, int size)
        {
            RegistrationNumber = registrationNumber;
            VehicleType = vehicleType;
            Size = size;
            ParkingTime = DateTime.Now;
        }

        public virtual string GetInfo()
        {
            return $"{VehicleType}: {RegistrationNumber}";
        }
    }
}
