using DVRP.Domain.Entities;

namespace DVRP.Application.Utilities;

public static class DvrpInstanceGenerator
{
    public static DvrpModel GenerateRandomInstance(int numCustomers, int numDepots, int numVehicles, double maxDemand,
        double maxServiceTime, double maxX, double maxY)
    {
        var customers = GenerateCustomers(numCustomers, maxDemand, maxServiceTime, maxX, maxY);
        var depots = GenerateDepots(numDepots, maxX, maxY);
        var vehicles = GenerateVehicles(numVehicles, numDepots);

        return new DvrpModel
        {
            Customers = customers,
            Depots = depots,
            Vehicles = vehicles
        };
    }

    private static List<Customer> GenerateCustomers(int numCustomers, double maxDemand, double maxServiceTime, double maxX, double maxY)
    {
        var customers = new List<Customer>(numCustomers);

        for (int i = 0; i < numCustomers; i++)
        {
            customers.Add(new Customer
            {
                Id = i + 1,
                X = Random.Shared.NextDouble() * maxX,
                Y = Random.Shared.NextDouble() * maxY,
                Demand = Random.Shared.NextDouble() * maxDemand,
                ServiceTime = Random.Shared.NextDouble() * maxServiceTime,
                TimeWindowStart = Random.Shared.NextDouble() * maxServiceTime,
                TimeWindowEnd = Random.Shared.NextDouble() * maxServiceTime + maxServiceTime
            });
        }

        return customers;
    }

    private static List<Depot> GenerateDepots(int numDepots, double maxX, double maxY)
    {
        var depots = new List<Depot>(numDepots);

        for (int i = 0; i < numDepots; i++)
        {
            depots.Add(new Depot
            {
                Id = i + 1,
                X = Random.Shared.NextDouble() * maxX,
                Y = Random.Shared.NextDouble() * maxY,
                StartTime = 0,
                EndTime = 24
            });
        }

        return depots;
    }

    private static List<Vehicle> GenerateVehicles(int numVehicles, int numDepots)
    {
        var vehicles = new List<Vehicle>(numVehicles);

        for (int i = 0; i < numVehicles; i++)
        {
            vehicles.Add(new Vehicle
            {
                Id = i + 1,
                Capacity = 100,
                DepotId = (i % numDepots) + 1
            });
        }

        return vehicles;
    }
}
