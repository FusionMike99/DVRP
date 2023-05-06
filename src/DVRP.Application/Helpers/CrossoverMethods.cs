using DVRP.Domain.Entities;

namespace DVRP.Application.Helpers;

public static class CrossoverMethods
{
    public static (DvrpSolution, DvrpSolution) OrderCrossover(DvrpModel model, DvrpSolution parent1, DvrpSolution parent2)
    {
        DvrpSolution offspring1 = new();
        DvrpSolution offspring2 = new();

        // Filter out empty routes
        List<VehicleRoute> nonEmptyParent1Routes = parent1.Routes.Where(r => r.Distance > 0).ToList();
        List<VehicleRoute> nonEmptyParent2Routes = parent2.Routes.Where(r => r.Distance > 0).ToList();

        int numRoutes = Math.Min(nonEmptyParent1Routes.Count, nonEmptyParent2Routes.Count);

        for (int routeIndex = 0; routeIndex < numRoutes; routeIndex++)
        {
            VehicleRoute parent1Route = nonEmptyParent1Routes[routeIndex];
            VehicleRoute parent2Route = nonEmptyParent2Routes[routeIndex];

            // Remove depots from parent routes.
            List<Customer> parent1Customers = parent1Route.Locations.OfType<Customer>().ToList();
            List<Customer> parent2Customers = parent2Route.Locations.OfType<Customer>().ToList();

            int customerCount = Math.Min(parent1Customers.Count, parent2Customers.Count);
            int crossoverPoint1 = Random.Shared.Next(customerCount);
            int crossoverPoint2 = Random.Shared.Next(customerCount);

            int start = Math.Min(crossoverPoint1, crossoverPoint2);
            int end = Math.Max(crossoverPoint1, crossoverPoint2);

            // Create offspring routes by copying the customer sequences between the crossover points.
            List<Customer> offspring1Customers = parent1Customers.GetRange(start, end - start);
            List<Customer> offspring2Customers = parent2Customers.GetRange(start, end - start);

            // Fill in the remaining customer sequences.
            FillRemainingCustomers(offspring1Customers, parent2Customers);
            FillRemainingCustomers(offspring2Customers, parent1Customers);

            // Create offspring vehicle routes.
            VehicleRoute offspring1Route = CreateVehicleRoute(parent1Route.Vehicle, model.Depots, offspring1Customers);
            VehicleRoute offspring2Route = CreateVehicleRoute(parent2Route.Vehicle, model.Depots, offspring2Customers);

            // Add the routes to the offspring solutions.
            offspring1.Routes.Add(offspring1Route);
            offspring2.Routes.Add(offspring2Route);
        }

        return (offspring1, offspring2);
    }

    public static (DvrpSolution, DvrpSolution) EdgeRecombinationCrossover(DvrpModel model, DvrpSolution parent1, DvrpSolution parent2)
    {
        DvrpSolution offspring1 = new();
        DvrpSolution offspring2 = new();

        // Filter out empty routes
        List<VehicleRoute> nonEmptyParent1Routes = parent1.Routes.Where(r => r.Distance > 0).ToList();
        List<VehicleRoute> nonEmptyParent2Routes = parent2.Routes.Where(r => r.Distance > 0).ToList();

        int numRoutes = Math.Min(nonEmptyParent1Routes.Count, nonEmptyParent2Routes.Count);

        for (int routeIndex = 0; routeIndex < numRoutes; routeIndex++)
        {
            VehicleRoute parent1Route = nonEmptyParent1Routes[routeIndex];
            VehicleRoute parent2Route = nonEmptyParent2Routes[routeIndex];

            // Remove depots from parent routes.
            List<Customer> parent1Customers = parent1Route.Locations.OfType<Customer>().ToList();
            List<Customer> parent2Customers = parent2Route.Locations.OfType<Customer>().ToList();

            // Create offspring customer sequences using ERX.
            List<Customer> offspring1Customers = EdgeRecombinationCrossoverHelper(parent1Customers, parent2Customers);
            List<Customer> offspring2Customers = EdgeRecombinationCrossoverHelper(parent2Customers, parent1Customers);

            // Create offspring vehicle routes.
            VehicleRoute offspring1Route = CreateVehicleRoute(parent1Route.Vehicle, model.Depots, offspring1Customers);
            VehicleRoute offspring2Route = CreateVehicleRoute(parent1Route.Vehicle, model.Depots, offspring2Customers);

            // Add the routes to the offspring solutions.
            offspring1.Routes.Add(offspring1Route);
            offspring2.Routes.Add(offspring2Route);
        }

        // Handle capacity constraints and repair offspring solutions if necessary.

        return (offspring1, offspring2);
    }

    private static List<Customer> EdgeRecombinationCrossoverHelper(List<Customer> parent1Customers, List<Customer> parent2Customers)
    {
        Dictionary<Customer, HashSet<Customer>> adjacencyList = new();
        foreach (Customer customer in parent1Customers.Concat(parent2Customers).Distinct())
        {
            HashSet<Customer> neighbors = new();
            int parent1Index = parent1Customers.IndexOf(customer);
            int parent2Index = parent2Customers.IndexOf(customer);

            if (parent1Index != -1)
            {
                if (parent1Index > 0)
                {
                    neighbors.Add(parent1Customers[parent1Index - 1]);
                }
                if (parent1Index < parent1Customers.Count - 1)
                {
                    neighbors.Add(parent1Customers[parent1Index + 1]);
                }
            }

            if (parent2Index != -1)
            {
                if (parent2Index > 0)
                {
                    neighbors.Add(parent2Customers[parent2Index - 1]);
                }
                if (parent2Index < parent2Customers.Count - 1)
                {
                    neighbors.Add(parent2Customers[parent2Index + 1]);
                }
            }

            adjacencyList[customer] = neighbors;
        }

        List<Customer> offspringCustomers = new();
        Customer currentCustomer = parent1Customers.First();
        while (offspringCustomers.Count < parent1Customers.Count)
        {
            offspringCustomers.Add(currentCustomer);
            adjacencyList.Remove(currentCustomer);

            foreach (HashSet<Customer> neighbors in adjacencyList.Values)
            {
                neighbors.Remove(currentCustomer);
            }

            if (adjacencyList.Count > 0)
            {
                currentCustomer = adjacencyList
                    .OrderBy(kvp => kvp.Value.Count)
                    .ThenBy(kvp => parent1Customers.IndexOf(kvp.Key))
                    .First().Key;
            }
        }

        return offspringCustomers;
    }

    private static void FillRemainingCustomers(List<Customer> offspringCustomers, List<Customer> parentCustomers)
    {
        // Create a list of remaining customers
        List<Customer> remainingCustomers = parentCustomers.Where(c => !offspringCustomers.Contains(c)).ToList();

        // Fisher-Yates shuffle for remaining customers
        int n = remainingCustomers.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (remainingCustomers[j], remainingCustomers[i]) = (remainingCustomers[i], remainingCustomers[j]);
        }

        // Insert shuffled remaining customers into the offspring customer sequence
        int insertionIndex = 0;
        foreach (Customer customer in remainingCustomers)
        {
            offspringCustomers.Insert(insertionIndex++, customer);
        }
    }

    private static VehicleRoute CreateVehicleRoute(Vehicle vehicle, List<Depot> depots, List<Customer> customers)
    {
        Depot startDepot = depots.First(depot => depot.Id == vehicle.DepotId) with { };
        List<Location> locations = new() { startDepot };
        locations.AddRange(customers);
        locations.Add(startDepot);

        return new VehicleRoute
        {
            Vehicle = vehicle with { },
            Locations = locations
        };
    }
}
