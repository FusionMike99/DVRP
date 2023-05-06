using DVRP.Domain.Entities;

namespace DVRP.Application.Helpers;

public static class MutationMethods
{
    public static void SwapMutation(DvrpSolution solution, double mutationRate)
    {
        if (Random.Shared.NextDouble() < mutationRate)
        {
            int routeIndex1 = Random.Shared.Next(solution.Routes.Count);
            int routeIndex2 = Random.Shared.Next(solution.Routes.Count);

            VehicleRoute route1 = solution.Routes[routeIndex1];
            VehicleRoute route2 = solution.Routes[routeIndex2];

            List<Customer> customers1 = route1.Locations.OfType<Customer>().ToList();
            List<Customer> customers2 = route2.Locations.OfType<Customer>().ToList();

            if (customers1.Count == 0 || customers2.Count == 0)
            {
                return; // Skip if one of the routes has no customers
            }

            int customerIndex1 = Random.Shared.Next(customers1.Count);
            int customerIndex2 = Random.Shared.Next(customers2.Count);

            Customer customer1 = customers1[customerIndex1];
            Customer customer2 = customers2[customerIndex2];

            // Skip if one of the customers is already in the other route
            if (route1.Locations.Contains(customer2) || route2.Locations.Contains(customer1))
            {
                return;
            }

            double newRoute1Demand = route1.Locations.OfType<Customer>().Sum(c => c.Demand) - customer1.Demand + customer2.Demand;
            double newRoute2Demand = route2.Locations.OfType<Customer>().Sum(c => c.Demand) - customer2.Demand + customer1.Demand;

            if (newRoute1Demand <= route1.Vehicle.Capacity && newRoute2Demand <= route2.Vehicle.Capacity)
            {
                // Swap customers
                route1.Locations[route1.Locations.IndexOf(customer1)] = customer2;
                route2.Locations[route2.Locations.IndexOf(customer2)] = customer1;
            }
        }
    }

    public static void InverseMutation(DvrpSolution solution, double mutationRate)
    {
        foreach (VehicleRoute route in solution.Routes)
        {
            if (Random.Shared.NextDouble() < mutationRate)
            {
                List<Customer> customers = route.Locations.OfType<Customer>().ToList();
                int customerCount = customers.Count;

                int mutationIndex1 = Random.Shared.Next(customerCount);
                int mutationIndex2 = Random.Shared.Next(customerCount);

                int start = Math.Min(mutationIndex1, mutationIndex2);
                int end = Math.Max(mutationIndex1, mutationIndex2);

                List<Customer> reversedCustomers = new List<Customer>(customers);
                reversedCustomers.Reverse(start, end - start);

                // Check if the mutated route violates capacity constraints
                double reversedRouteDemand = reversedCustomers.Sum(c => c.Demand);
                if (reversedRouteDemand > route.Vehicle.Capacity)
                {
                    // If capacity constraints are violated, discard the mutation
                    continue;
                }

                // Reconstruct the mutated route
                List<Location> mutatedLocations = new() { route.Locations[^1] };
                mutatedLocations.AddRange(reversedCustomers);
                mutatedLocations.Add(route.Locations[^1]);
                route.Locations = mutatedLocations;
            }
        }
    }
}
