using System;

namespace PragueParkingAccess
{
    public class VehicleSaveData
    {
        public string RegistrationNumber { get; init; }
        public string VehicleType { get; init; }
        public DateTime ParkingTime { get; init; }
        public int ParkingSpot { get; init; }

        public override string ToString()
        {
            return $"{VehicleType} {RegistrationNumber} parked at spot {ParkingSpot} since {ParkingTime}.";
        }
    }
}
