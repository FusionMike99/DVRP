using DVRP.Domain.Entities;

namespace DVRP.Application.Helpers;

public static class InitializationMethods
{
    public static DvrpSolution NearestNeighborInitialization(DvrpModel model)
    {
        var routes = new List<VehicleRoute>();
        var remainingCustomers = new List<Customer>(model.Customers);

        while (remainingCustomers.Count > 0)
        {
            foreach (var vehicle in model.Vehicles)
            {
                if (remainingCustomers.Count == 0) break;

                var depot = model.Depots.First(d => d.Id == vehicle.DepotId);
                var route = new VehicleRoute { Vehicle = vehicle, Locations = new() { depot } };  // Start from the depot
                Location currentLocation = depot with { };
                double currentCapacity = vehicle.Capacity;

                while (remainingCustomers.Count > 0 && currentCapacity > 0)
                {
                    var nearestCustomer = remainingCustomers.OrderBy(c => c.CalculateDistance(currentLocation)).First();

                    if (nearestCustomer.Demand <= currentCapacity)
                    {
                        route.Locations.Add(nearestCustomer);
                        currentCapacity -= nearestCustomer.Demand;
                        remainingCustomers.Remove(nearestCustomer);
                        currentLocation = nearestCustomer;
                    }
                    else
                    {
                        break;
                    }
                }

                route.Locations.Add(depot);  // Return to the depot
                routes.Add(route);
            }
        }

        return new DvrpSolution { Routes = routes };
    }

    public static DvrpSolution HybridInitialization(DvrpModel model, double randomizationRatio)
    {
        var solution = NearestNeighborInitialization(model);

        int routesToRandomize = (int)(randomizationRatio * solution.Routes.Count);

        for (int i = 0; i < routesToRandomize; i++)
        {
            var route = solution.Routes[i];

            List<Customer> customers = route.Locations.OfType<Customer>().ToList();

            // Fisher-Yates shuffle for customer locations
            int n = customers.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Shared.Next(n + 1);
                (customers[n], customers[k]) = (customers[k], customers[n]);
            }

            // Reconstruct the mutated route
            List<Location> mutatedLocations = new() { route.Locations[^1] };
            mutatedLocations.AddRange(customers);
            mutatedLocations.Add(route.Locations[^1]);
            route.Locations = mutatedLocations;
        }

        return solution;
    }
}
