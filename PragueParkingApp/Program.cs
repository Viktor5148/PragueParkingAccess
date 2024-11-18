using System;
using System.Collections.Generic;
using System.IO;
using PragueParkingAccess;
using Spectre.Console; 

namespace PragueParkingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string priceListFilePath = "pricelist.txt";
            Dictionary<string, int> priceList = LoadPriceList(priceListFilePath);

            if (priceList.Count > 0)
            {
                Console.WriteLine("Price list loaded successfully:");
                foreach (var item in priceList)
                {
                    Console.WriteLine($"{item.Key}: {item.Value} CZK per hour");
                }
            }
            else
            {
                Console.WriteLine("Failed to load price list.");
            }

            
            var vehicleTypes = new List<VehicleType>
            {
                new VehicleType { Type = "CAR", Size = 4 },
                new VehicleType { Type = "MC", Size = 2 }
            };
            var garage = new ParkingGarage(10, 4, vehicleTypes);

            
            bool exit = false;
            while (!exit)
            {
                string choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold blue]Choose an option:[/]")
                        .AddChoices(new[]
                        {
                            "1. Park a vehicle",
                            "2. Show parking spots",
                            "3. Move a vehicle",
                            "4. Find a vehicle",
                            "5. Remove a vehicle",
                            "6. Exit"
                        }));

                switch (choice[0])
                {
                    case '1': 
                        Console.WriteLine("Enter vehicle type (CAR/MC):");
                        string type = Console.ReadLine().ToUpper();
                        Console.WriteLine("Enter registration number:");
                        string regNumber = Console.ReadLine().ToUpper();

                        Vehicle vehicle = type switch
                        {
                            "CAR" => new Car(regNumber),
                            "MC" => new MC(regNumber),
                            _ => null
                        };

                        if (vehicle != null)
                        {
                            garage.ParkVehicle(vehicle);
                        }
                        else
                        {
                            Console.WriteLine("Invalid vehicle type.");
                        }
                        break;

                    case '2': 
                        garage.ShowParkingLot();
                        break;

                    case '3': 
                        Console.WriteLine("Enter registration number:");
                        string regToMove = Console.ReadLine().ToUpper();
                        Console.WriteLine("Enter new parking spot:");
                        if (int.TryParse(Console.ReadLine(), out int newSpot))
                        {
                            garage.MoveVehicle(regToMove, newSpot);
                        }
                        else
                        {
                            Console.WriteLine("Invalid spot.");
                        }
                        break;

                    case '4': 
                        Console.WriteLine("Enter registration number:");
                        string regToFind = Console.ReadLine().ToUpper();
                        garage.FindVehicle(regToFind);
                        break;

                    case '5': 
                        Console.WriteLine("Enter registration number:");
                        string regToRemove = Console.ReadLine().ToUpper();
                        var removedVehicle = garage.RemoveVehicle(regToRemove);
                        if (removedVehicle != null)
                        {
                            int price = CalculateParkingPrice(removedVehicle, priceList);
                            Console.WriteLine($"Vehicle {removedVehicle.RegistrationNumber} must pay {price} CZK.");
                        }
                        break;

                    case '6': 
                        exit = true;
                        Console.WriteLine("Exiting the program...");
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

       
        static Dictionary<string, int> LoadPriceList(string filePath)
        {
            var priceList = new Dictionary<string, int>();

            try
            {
                foreach (var line in File.ReadAllLines(filePath))
                {
                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int price))
                        {
                            priceList[parts[0]] = price;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading the price list: {ex.Message}");
            }

            return priceList;
        }

        
        static int CalculateParkingPrice(Vehicle vehicle, Dictionary<string, int> priceList)
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

            Console.WriteLine($"No price information available for vehicle type {vehicle.VehicleType}.");
            return 0;
        }
    }
}
