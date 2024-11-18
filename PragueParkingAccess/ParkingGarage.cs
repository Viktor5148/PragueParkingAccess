using System.Text.Json;
using PragueParkingAccess;
using Spectre.Console;

namespace PragueParkingAccess
{
    public class ParkingGarage
    {
        private List<Vehicle>[] parkingLot;
        private string saveFilePath = "../../../parkingData.json";
        private int parkingSpotSize;

        public ParkingGarage(int spots, int spotSize, List<VehicleType> vehicleTypes)
        {
            parkingSpotSize = spotSize;
            parkingLot = new List<Vehicle>[spots];
            for (int i = 0; i < parkingLot.Length; i++)
            {
                parkingLot[i] = new List<Vehicle>();
            }
            LoadVehicles();
        }

        public void ParkVehicle(Vehicle vehicle)
        {
            int startSpot = FindAvailableSpots(vehicle);

            if (startSpot == -1)
            {
                Console.WriteLine("No available parking spot for the vehicle.");
                return;
            }

            int requiredSpots = (int)Math.Ceiling(vehicle.Size / (double)parkingSpotSize);

            for (int i = startSpot; i < startSpot + requiredSpots; i++)
            {
                parkingLot[i].Add(vehicle);
            }

            if (requiredSpots == 1)
            {
                Console.WriteLine($"{vehicle.VehicleType} {vehicle.RegistrationNumber} has been parked on parking spot {startSpot + 1}.");
            }
            else
            {
                Console.WriteLine($"{vehicle.VehicleType} {vehicle.RegistrationNumber} has been parked from spot {startSpot + 1} to {startSpot + requiredSpots}.");
            }

            SaveVehicles();
        }

        private int FindAvailableSpots(Vehicle vehicle)
        {
            int vehicleSize = vehicle.Size;
            int requiredSpots = (int)Math.Ceiling(vehicleSize / (double)parkingSpotSize);

            if (vehicleSize <= parkingSpotSize / 2)
            {
                for (int i = 0; i < parkingLot.Length; i++)
                {
                    int currentSizeUsed = parkingLot[i].Sum(v => v.Size);
                    if (currentSizeUsed + vehicleSize <= parkingSpotSize)
                    {
                        return i;
                    }
                }
            }

            int consecutiveEmpty = 0;
            for (int i = 0; i < parkingLot.Length; i++)
            {
                if (parkingLot[i].Count == 0)
                {
                    consecutiveEmpty++;
                    if (consecutiveEmpty == requiredSpots)
                    {
                        return i - requiredSpots + 1;
                    }
                }
                else
                {
                    consecutiveEmpty = 0;
                }
            }

            return -1;
        }

        private Configuration LoadConfig()
        {
            string configPath = "../../../config.json";
            if (File.Exists(configPath))
            {
                string jsonData = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<Configuration>(jsonData);
            }
            return new Configuration { ParkingSpots = 100, Columns = 10 };
        }

        public void ShowParkingLot()
        {
            Configuration config = LoadConfig();
            int columns = config.Columns;
            int totalSpots = config.ParkingSpots;
            var table = new Table().NoBorder();

            AnsiConsole.MarkupLine("[red]Red = Occupied[/], [yellow]Yellow = There is still room for one MC[/], [green]Green = Empty[/]");

            for (int i = 0; i < columns; i++)
            {
                table.AddColumn("");
            }

            var rowContent = new List<string>();

            for (int spotIndex = 0; spotIndex < totalSpots; spotIndex++)
            {
                string cellContent = "";

                if (spotIndex < parkingLot.Length && parkingLot[spotIndex].Count > 0)
                {
                    var vehiclesOnSpot = parkingLot[spotIndex];

                    if (vehiclesOnSpot.Count == 1)
                    {
                        if (vehiclesOnSpot[0].VehicleType == "MC")
                        {
                            cellContent = $"[yellow]#{spotIndex + 1}\n{Markup.Escape(vehiclesOnSpot[0].RegistrationNumber)}[/]";
                        }
                        else
                        {
                            cellContent = $"[red]#{spotIndex + 1}\n{Markup.Escape(vehiclesOnSpot[0].RegistrationNumber)}[/]";
                        }
                    }
                    else if (vehiclesOnSpot.Count == 2)
                    {
                        if (vehiclesOnSpot.All(v => v.VehicleType == "MC"))
                        {
                            cellContent = $"[red]#{spotIndex + 1}\n{Markup.Escape(vehiclesOnSpot[0].RegistrationNumber)}, {Markup.Escape(vehiclesOnSpot[1].RegistrationNumber)}[/]";
                        }
                        else
                        {
                            cellContent = $"[red]#{spotIndex + 1}\n{Markup.Escape(vehiclesOnSpot[0].RegistrationNumber)}, {Markup.Escape(vehiclesOnSpot[1].RegistrationNumber)}[/]";
                        }
                    }
                }
                else
                {
                    cellContent = $"[green]#{spotIndex + 1}\nEmpty[/]";
                }

                rowContent.Add(cellContent);

                if (rowContent.Count == columns)
                {
                    table.AddRow(rowContent.ToArray());
                    rowContent.Clear();
                }
            }

            if (rowContent.Count > 0)
            {
                while (rowContent.Count < columns)
                {
                    rowContent.Add("EMPTY");
                }
                table.AddRow(rowContent.ToArray());
            }

            AnsiConsole.Write(table);
        }

        public void MoveVehicle(string registrationNumber, int newSpot)
        {
            if (newSpot < 1 || newSpot > parkingLot.Length)
            {
                Console.WriteLine("Invalid number.");
                return;
            }
            newSpot--;

            for (int i = 0; i < parkingLot.Length; i++)
            {
                for (int j = 0; j < parkingLot[i].Count; j++)
                {
                    if (parkingLot[i][j].RegistrationNumber == registrationNumber)
                    {
                        if (parkingLot[newSpot].Count == 0 ||
                            (parkingLot[i][j] is MC && parkingLot[newSpot][0] is MC && parkingLot[newSpot].Count < 2))
                        {
                            parkingLot[newSpot].Add(parkingLot[i][j]);
                            parkingLot[i].RemoveAt(j);
                            Console.WriteLine($"Vehicle {registrationNumber} has been moved to spot {newSpot + 1}.");
                        }
                        else
                        {
                            Console.WriteLine("The new spot is not available.");
                        }
                        return;
                    }
                }
            }
            Console.WriteLine("The vehicle could not be found.");
        }

        public void FindVehicle(string registrationNumber)
        {
            for (int i = 0; i < parkingLot.Length; i++)
            {
                foreach (var vehicle in parkingLot[i])
                {
                    if (vehicle.RegistrationNumber == registrationNumber)
                    {
                        Console.WriteLine($"Vehicle {registrationNumber} was found in spot {i + 1}.");
                        return;
                    }
                }
            }
            Console.WriteLine("The vehicle could not be found.");
        }

        public Vehicle RemoveVehicle(string registrationNumber)
        {
            for (int i = 0; i < parkingLot.Length; i++)
            {
                for (int j = 0; j < parkingLot[i].Count; j++)
                {
                    if (parkingLot[i][j].RegistrationNumber == registrationNumber)
                    {
                        Vehicle vehicleToRemove = parkingLot[i][j];
                        parkingLot[i].RemoveAt(j);
                        Console.WriteLine($"Vehicle {registrationNumber} has been removed from spot {i + 1}.");
                        SaveVehicles();
                        return vehicleToRemove;
                    }
                }
            }
            Console.WriteLine("The vehicle could not be found.");
            return null;
        }

        public void SaveVehicles()
        {
            var vehiclesToSave = new List<VehicleSaveData>();

            for (int i = 0; i < parkingLot.Length; i++)
            {
                foreach (var vehicle in parkingLot[i])
                {
                    vehiclesToSave.Add(new VehicleSaveData
                    {
                        RegistrationNumber = vehicle.RegistrationNumber,
                        VehicleType = vehicle.VehicleType,
                        ParkingTime = vehicle.ParkingTime,
                        ParkingSpot = i
                    });
                }
            }

            string jsonData = JsonSerializer.Serialize(vehiclesToSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(saveFilePath, jsonData);
            Console.WriteLine("Parkinglot has been saved to file.");
        }

        public void LoadVehicles()
        {
            if (File.Exists(saveFilePath))
            {
                string jsonData = File.ReadAllText(saveFilePath);
                var loadedVehicles = JsonSerializer.Deserialize<List<VehicleSaveData>>(jsonData);

                if (loadedVehicles != null)
                {
                    foreach (var vehicleData in loadedVehicles)
                    {
                        Vehicle vehicle = vehicleData.VehicleType switch
                        {
                            "CAR" => new Car(vehicleData.RegistrationNumber),
                            "MC" => new MC(vehicleData.RegistrationNumber),
                            _ => null
                        };

                        if (vehicle != null)
                        {
                            vehicle.ParkingTime = vehicleData.ParkingTime;
                            parkingLot[vehicleData.ParkingSpot].Add(vehicle);
                        }
                    }
                    Console.WriteLine("Parked vehicles have been loaded from file.");
                }
            }
        }
    }
}
namespace PragueParkingAccess
{
    public class PragueParkingAccess
    {
        

        public static int CalculateParkingPrice(Vehicle vehicle, Dictionary<string, int> priceList)
        {
            TimeSpan parkingDuration = DateTime.Now - vehicle.ParkingTime;

            if (parkingDuration.TotalMinutes <= 10)
            {
                return 0; 
            }

            if (priceList.TryGetValue(vehicle.VehicleType, out int hourlyRate))
            {
                int hoursParked = (int)Math.Ceiling(parkingDuration.TotalHours);
                return hoursParked * hourlyRate;
            }

            Console.WriteLine($"No price information available for the vehicle type {vehicle.VehicleType}.");
            return 0;
        }
    }
}

