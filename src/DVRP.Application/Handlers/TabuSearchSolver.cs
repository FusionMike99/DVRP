using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;

namespace DVRP.Application.Handlers;

public class TabuSearchSolver : IDvrpSolver
{
    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not TabuSearchParameters tabuParameters)
        {
            throw new ArgumentException("Invalid parameters for TabuSearchSolver", nameof(parameters));
        }

        var tabuList = new List<DvrpSolution>(tabuParameters.TabuListSize);
        initialSolution ??= GenerateInitialSolution(model); // Use a problem-specific construction heuristic
        var bestSolution = initialSolution.Clone();

        for (int iteration = 0; iteration < tabuParameters.MaxIterations; iteration++)
        {
            var neighbors = GenerateNeighbors(initialSolution, model, tabuParameters);

            var bestNeighbor = neighbors
                .Where(neighbor => !tabuList.Contains(neighbor))
                .OrderBy(neighbor => neighbor.Fitness)
                .FirstOrDefault();

            if (bestNeighbor == null)
            {
                // Apply diversification strategy if no suitable neighbor is found
                initialSolution = ApplyDiversification(initialSolution, model, tabuParameters);
                continue;
            }

            tabuList.Add(initialSolution);
            if (tabuList.Count > tabuParameters.TabuListSize)
            {
                tabuList.RemoveAt(0);
            }

            // Apply intensification strategy if the current solution improves
            if (bestNeighbor.Fitness < bestSolution.Fitness)
            {
                bestSolution = bestNeighbor;
                initialSolution = ApplyIntensification(bestNeighbor, model, tabuParameters);
            }
        }

        return bestSolution;
    }

    public static DvrpSolution GenerateInitialSolution(DvrpModel model)
    {
        var savings = CalculateSavings(model);
        var routes = CreateRoutesFromSavings(model, savings);
        var solution = new DvrpSolution { Routes = routes };
        solution.CalculateFitness(model.Depots.Count);
        return solution;
    }

    private static List<(Customer, Customer, double)> CalculateSavings(DvrpModel model)
    {
        var savings = new List<(Customer, Customer, double)>();
        foreach (var c1 in model.Customers)
        {
            foreach (var c2 in model.Customers)
            {
                if (string.Compare(c1.Id, c2.Id) < 0)
                {
                    var closestDepotToC1 = model.Depots.OrderBy(d => d.CalculateDistance(c1)).First();
                    var closestDepotToC2 = model.Depots.OrderBy(d => d.CalculateDistance(c2)).First();

                    double saving = closestDepotToC1.CalculateDistance(c1) +
                                    closestDepotToC2.CalculateDistance(c2) -
                                    c1.CalculateDistance(c2);
                    savings.Add((c1, c2, saving));
                }
            }
        }

        return savings.OrderByDescending(s => s.Item3).ToList();
    }

    private static List<VehicleRoute> CreateRoutesFromSavings(DvrpModel model, List<(Customer, Customer, double)> savings)
    {
        var routes = new List<VehicleRoute>();
        var unassignedCustomers = new HashSet<Customer>(model.Customers);

        foreach (var vehicle in model.Vehicles)
        {
            var depot = model.Depots.First(d => d.Id == vehicle.DepotId);  // Get the depot associated with the vehicle
            var route = new VehicleRoute { Vehicle = vehicle, Locations = new() { depot } };  // Start from the associated depot
            double currentCapacity = vehicle.Capacity;

            while (unassignedCustomers.Count > 0)
            {
                var saving = savings.FirstOrDefault(s =>
                    unassignedCustomers.Contains(s.Item1) && unassignedCustomers.Contains(s.Item2) &&
                    s.Item1.Demand + s.Item2.Demand <= currentCapacity);

                if (saving == default)
                    break;

                route.Locations.Add(saving.Item1);
                route.Locations.Add(saving.Item2);
                currentCapacity -= saving.Item1.Demand + saving.Item2.Demand;
                unassignedCustomers.Remove(saving.Item1);
                unassignedCustomers.Remove(saving.Item2);

                savings.RemoveAll(s => s.Item1 == saving.Item1 || s.Item1 == saving.Item2 ||
                                       s.Item2 == saving.Item1 || s.Item2 == saving.Item2);
            }

            route.Locations.Add(depot);  // Return to the associated depot
            if (route.Locations.Count > 2)  // Exclude routes with only depots
            {
                routes.Add(route);
            }
        }

        // If any customers remain unassigned, assign them to the nearest depot in a single-visit route
        while (unassignedCustomers.Count > 0)
        {
            foreach (var vehicle in model.Vehicles)
            {
                if (unassignedCustomers.Count == 0) break;

                var depot = model.Depots.First(d => d.Id == vehicle.DepotId);
                var route = new VehicleRoute { Vehicle = vehicle, Locations = new() { depot } };  // Start from the depot
                Location currentLocation = depot with { };
                double currentCapacity = vehicle.Capacity;

                while (unassignedCustomers.Count > 0 && currentCapacity > 0)
                {
                    var nearestCustomer = unassignedCustomers.OrderBy(c => c.CalculateDistance(currentLocation)).First();

                    if (nearestCustomer.Demand <= currentCapacity)
                    {
                        route.Locations.Add(nearestCustomer);
                        currentCapacity -= nearestCustomer.Demand;
                        unassignedCustomers.Remove(nearestCustomer);
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

        return routes;
    }


    private static List<DvrpSolution> GenerateNeighbors(DvrpSolution currentSolution, DvrpModel model, TabuSearchParameters tabuParameters)
    {
        var neighbors = new List<DvrpSolution>();

        foreach (var route1 in currentSolution.Routes)
        {
            var route1Customers = route1.Locations.OfType<Customer>().ToList();

            foreach (var customer1 in route1Customers)
            {
                int customer1Index = route1.Locations.IndexOf(customer1);

                // Relocate
                foreach (var route2 in currentSolution.Routes)
                {
                    if (route1 == route2)
                    {
                        continue;
                    }

                    // Check capacity constraint
                    if (route2.Locations.OfType<Customer>().Sum(c => c.Demand) + customer1.Demand <= route2.Vehicle.Capacity)
                    {
                        var relocatedSolution = currentSolution.Clone();
                        var relocatedRoutes1 = relocatedSolution.Routes.Where(r => r.Vehicle.Id == route1.Vehicle.Id).ToList();
                        var relocatedRoutes2 = relocatedSolution.Routes.Where(r => r.Vehicle.Id == route2.Vehicle.Id).ToList();

                        foreach (var relocatedRoute1 in relocatedRoutes1)
                        {
                            if (relocatedRoute1.Locations.Contains(customer1))
                            {
                                // Move customer1 from route1 to route2
                                relocatedRoute1.Locations.Remove(customer1);
                                foreach (var relocatedRoute2 in relocatedRoutes2)
                                {
                                    relocatedRoute2.Locations.Insert(relocatedRoute2.Locations.Count - 1, customer1);
                                }

                                // Update fitness
                                relocatedSolution.CalculateFitness(model.Depots.Count);

                                neighbors.Add(relocatedSolution);
                                break;
                            }
                        }
                    }
                }

                // Swap and two-opt
                foreach (var customer2 in route1Customers)
                {
                    if (customer1 == customer2)
                    {
                        continue;
                    }

                    int customer2Index = route1.Locations.IndexOf(customer2);

                    // Swap
                    var swappedSolution = currentSolution.Clone();
                    var swappedRoutes = swappedSolution.Routes.Where(r => r.Vehicle.Id == route1.Vehicle.Id).ToList();

                    foreach (var swappedRoute in swappedRoutes)
                    {
                        if (swappedRoute.Locations.Contains(customer1) && swappedRoute.Locations.Contains(customer2))
                        {
                            swappedRoute.Locations[customer1Index] = customer2;
                            swappedRoute.Locations[customer2Index] = customer1;

                            // Update fitness
                            swappedSolution.CalculateFitness(model.Depots.Count);

                            neighbors.Add(swappedSolution);
                            break;
                        }
                    }

                    // Two-opt
                    var twoOptSolution = currentSolution.Clone();
                    var twoOptRoutes = twoOptSolution.Routes.Where(r => r.Vehicle.Id == route1.Vehicle.Id).ToList();

                    foreach (var twoOptRoute in twoOptRoutes)
                    {
                        if (twoOptRoute.Locations.Contains(customer1) && twoOptRoute.Locations.Contains(customer2))
                        {
                            twoOptRoute.Locations.Reverse(Math.Min(customer1Index, customer2Index), Math.Abs(customer1Index - customer2Index) + 1);

                            // Update fitness
                            twoOptSolution.CalculateFitness(model.Depots.Count);

                            neighbors.Add(twoOptSolution);
                            break;
                        }
                    }
                }
            }
        }

        // Limit the number of neighbors based on the neighborhood search size
        var limitedNeighbors = neighbors.OrderBy(n => n.Fitness).Take(tabuParameters.NeighborhoodSearchSize).ToList();

        return limitedNeighbors;
    }

    private static DvrpSolution ApplyDiversification(DvrpSolution currentSolution, DvrpModel model, TabuSearchParameters tabuParameters)
    {
        var diversifiedSolution = currentSolution.Clone();

        // Determine the number of swaps to perform based on the diversification factor
        int numSwaps = (int)(diversifiedSolution.Routes.Count * tabuParameters.DiversificationFactor);

        for (int i = 0; i < numSwaps; i++)
        {
            var route1 = diversifiedSolution.Routes[Random.Shared.Next(diversifiedSolution.Routes.Count)];
            var route2 = diversifiedSolution.Routes[Random.Shared.Next(diversifiedSolution.Routes.Count)];

            if (route1 == route2)
            {
                continue;
            }

            var customersInRoute1 = route1.Locations.OfType<Customer>().ToList();
            var customersInRoute2 = route2.Locations.OfType<Customer>().ToList();

            if (customersInRoute1.Count == 0 || customersInRoute2.Count == 0)
            {
                continue;
            }

            int customerIndex1 = Random.Shared.Next(customersInRoute1.Count);
            int customerIndex2 = Random.Shared.Next(customersInRoute2.Count);

            var customer1 = customersInRoute1[customerIndex1];
            var customer2 = customersInRoute2[customerIndex2];

            double newRoute1Demand = route1.Locations.OfType<Customer>().Sum(c => c.Demand) - customer1.Demand + customer2.Demand;
            double newRoute2Demand = route2.Locations.OfType<Customer>().Sum(c => c.Demand) - customer2.Demand + customer1.Demand;

            // Check capacity constraint
            if (newRoute1Demand <= route1.Vehicle.Capacity && newRoute2Demand <= route2.Vehicle.Capacity)
            {
                int route1Index = route1.Locations.IndexOf(customer1);
                int route2Index = route2.Locations.IndexOf(customer2);
                route1.Locations[route1Index] = customer2;
                route2.Locations[route2Index] = customer1;
            }
        }

        diversifiedSolution.CalculateFitness(model.Depots.Count);

        return diversifiedSolution;
    }

    private static DvrpSolution ApplyIntensification(DvrpSolution currentSolution, DvrpModel model, TabuSearchParameters tabuParameters)
    {
        var intensifiedSolution = currentSolution.Clone();

        // Determine the number of routes to intensify based on the intensification factor
        int numRoutesToIntensify = (int)(intensifiedSolution.Routes.Count * tabuParameters.IntensificationFactor);

        // Select the best routes
        var bestRoutes = intensifiedSolution.Routes.OrderBy(r => r.Distance).Take(numRoutesToIntensify);

        foreach (var route in bestRoutes)
        {
            for (int i = 1; i < route.Locations.Count - 2; i++)
            {
                for (int j = i + 1; j < route.Locations.Count - 1; j++)
                {
                    var twoOptSolution = intensifiedSolution.Clone();
                    var twoOptRoute = twoOptSolution.Routes.First(r => r.Vehicle.Id == route.Vehicle.Id);

                    // Apply 2-opt
                    twoOptRoute.Locations.Reverse(i, j - i + 1);

                    // Update fitness
                    twoOptSolution.CalculateFitness(model.Depots.Count);

                    // If the new solution is better, update the intensified solution
                    if (twoOptSolution.Fitness < intensifiedSolution.Fitness)
                    {
                        intensifiedSolution = twoOptSolution;
                    }
                }
            }
        }

        return intensifiedSolution;
    }
}
