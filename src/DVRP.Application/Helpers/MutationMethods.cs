using DVRP.Domain.Entities;
using System.Reflection;

namespace DVRP.Application.Helpers;

public static class MutationMethods
{
    public static void SwapMutation(DvrpSolution solution, double mutationRate)
    {
        int numRoutes = solution.Routes.Count;

        for (int i = 0; i < numRoutes; i++)
        {
            if (Random.Shared.NextDouble() < mutationRate)
            {
                VehicleRoute route1 = solution.Routes[i];
                VehicleRoute route2;

                // Ensure the selected routes belong to different depots
                do
                {
                    route2 = solution.Routes[Random.Shared.Next(numRoutes)];
                } while (route1.Vehicle.DepotId == route2.Vehicle.DepotId);

                // Randomly select customers from each route
                int index1 = Random.Shared.Next(1, route1.Locations.Count - 1);
                int index2 = Random.Shared.Next(1, route2.Locations.Count - 1);

                Customer customer1 = route1.Locations[index1] as Customer;
                Customer customer2 = route2.Locations[index2] as Customer;

                // Check if swapping the customers would violate the capacity constraints
                double newRoute1Demand = route1.Locations.OfType<Customer>().Sum(c => c.Demand) - customer1.Demand + customer2.Demand;
                double newRoute2Demand = route2.Locations.OfType<Customer>().Sum(c => c.Demand) - customer2.Demand + customer1.Demand;

                if (newRoute1Demand <= route1.Vehicle.Capacity && newRoute2Demand <= route2.Vehicle.Capacity)
                {
                    // Perform the swap
                    route1.Locations[index1] = customer2;
                    route2.Locations[index2] = customer1;
                }
            }
        }
    }

    public static void InverseMutation(DvrpSolution solution, double mutationRate)
    {
        foreach (VehicleRoute route in solution.Routes)
        {
            if (Random.Shared.NextDouble() <= mutationRate)
            {
                List<Customer> customers = route.Locations.OfType<Customer>().ToList();
                int customerCount = customers.Count;

                int mutationIndex1 = Random.Shared.Next(customerCount);
                int mutationIndex2 = Random.Shared.Next(customerCount);

                int start = Math.Min(mutationIndex1, mutationIndex2);
                int end = Math.Max(mutationIndex1, mutationIndex2);

                customers.Reverse(start, end - start);

                // Repair the solution if capacity constraints are violated
                // ... implement a repair strategy if needed

                // Reconstruct the mutated route
                List<Location> mutatedLocations = new() { route.Locations[^1] };
                mutatedLocations.AddRange(customers);
                mutatedLocations.Add(route.Locations[^1]);
                route.Locations = mutatedLocations;
            }
        }
    }
}
