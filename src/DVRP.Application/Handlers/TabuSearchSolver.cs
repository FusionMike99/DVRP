using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class TabuSearchSolver : IDvrpSolver
{
    public Algorithm Algorithm { get; } = Algorithm.TabuSearch;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not TabuSearchParameters tabuParameters)
        {
            throw new ArgumentException("Invalid parameters for TabuSearchSolver", nameof(parameters));
        }

        var tabuList = new List<DvrpSolution>(tabuParameters.TabuListSize);
        var currentSolution = GenerateInitialSolution(model); // Use a problem-specific construction heuristic
        var bestSolution = currentSolution.Clone();

        for (int iteration = 0; iteration < tabuParameters.MaxIterations; iteration++)
        {
            var neighbors = GenerateNeighbors(currentSolution, model, tabuParameters);

            var bestNeighbor = neighbors
                .Where(neighbor => !tabuList.Contains(neighbor))
                .OrderBy(neighbor => neighbor.Fitness)
                .FirstOrDefault();

            if (bestNeighbor == null)
            {
                // Apply diversification strategy if no suitable neighbor is found
                currentSolution = ApplyDiversification(currentSolution, model, tabuParameters);
                continue;
            }

            if (bestNeighbor.Fitness < bestSolution.Fitness)
            {
                bestSolution = bestNeighbor;
            }

            tabuList.Add(currentSolution);
            if (tabuList.Count > tabuParameters.TabuListSize)
            {
                tabuList.RemoveAt(0);
            }

            currentSolution = bestNeighbor;

            // Apply intensification strategy if the current solution improves
            if (currentSolution.Fitness < bestSolution.Fitness)
            {
                currentSolution = ApplyIntensification(currentSolution, model, tabuParameters);
            }
        }

        return bestSolution;
    }

    private static DvrpSolution GenerateInitialSolution(DvrpModel model)
    {
        var routes = new List<VehicleRoute>();

        // Calculate the savings matrix
        var savings = CalculateSavingsMatrix(model.Customers, model.Depots);

        // Sort the savings in decreasing order
        var sortedSavings = savings.OrderByDescending(s => s.Saving);

        var assignedCustomers = new HashSet<Customer>();

        // Create a copy of the original vehicles list to avoid modifying it
        var vehicles = model.Vehicles.Select(v => v with { }).ToList();

        foreach (var (Customer1, Customer2, Saving) in sortedSavings)
        {
            if (assignedCustomers.Contains(Customer1) || assignedCustomers.Contains(Customer2))
            {
                continue;
            }

            var vehicle = vehicles
                .Where(v => v.Capacity >= Customer1.Demand + Customer2.Demand)
                .OrderBy(v => v.Capacity)
                .FirstOrDefault();

            if (vehicle != null)
            {
                var depot = model.Depots.First(d => d.Id == vehicle.DepotId);
                var route = new VehicleRoute
                {
                    Vehicle = vehicle,
                    Locations = new List<Location> { depot, Customer1, Customer2, depot }
                };
                routes.Add(route);
                assignedCustomers.Add(Customer1);
                assignedCustomers.Add(Customer2);

                // Reduce the vehicle's capacity
                vehicle.Capacity -= Customer1.Demand + Customer2.Demand;
            }
        }

        // Assign remaining unassigned customers to routes, respecting the capacity constraint
        foreach (var customer in model.Customers.Except(assignedCustomers))
        {
            var closestDepot = model.Depots.OrderBy(d => d.CalculateDistance(customer)).First();
            var route = routes
                .Where(r => r.Vehicle.DepotId == closestDepot.Id && r.Locations.OfType<Customer>().Sum(c => c.Demand) + customer.Demand <= r.Vehicle.Capacity)
                .OrderBy(r => r.Locations.Count)
                .FirstOrDefault();

            if (route != null)
            {
                route.Locations.Insert(route.Locations.Count - 1, customer);

                // Reduce the vehicle's capacity
                route.Vehicle.Capacity -= customer.Demand;
            }
        }

        var solution = new DvrpSolution { Routes = routes };
        solution.CalculateFitness(model.Depots.Count);
        return solution;
    }

    private static List<(Customer Customer1, Customer Customer2, double Saving)> CalculateSavingsMatrix(List<Customer> customers, List<Depot> depots)
    {
        var savings = new List<(Customer, Customer, double)>();

        for (int i = 0; i < customers.Count; i++)
        {
            var customer1 = customers[i];

            for (int j = i + 1; j < customers.Count; j++)
            {
                var customer2 = customers[j];
                var closestDepot1 = depots.OrderBy(d => d.CalculateDistance(customer1)).First();
                var closestDepot2 = depots.OrderBy(d => d.CalculateDistance(customer2)).First();

                double saving = closestDepot1.CalculateDistance(customer1) + closestDepot2.CalculateDistance(customer2) - customer1.CalculateDistance(customer2);
                savings.Add((customer1, customer2, saving));
            }
        }

        return savings;
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
                        var relocatedRoute1 = relocatedSolution.Routes.Single(r => r.Vehicle.Id == route1.Vehicle.Id);
                        var relocatedRoute2 = relocatedSolution.Routes.Single(r => r.Vehicle.Id == route2.Vehicle.Id);

                        // Move customer1 from route1 to route2
                        relocatedRoute1.Locations.Remove(customer1);
                        relocatedRoute2.Locations.Insert(relocatedRoute2.Locations.Count - 1, customer1);

                        // Update fitness
                        relocatedSolution.CalculateFitness(model.Depots.Count);

                        neighbors.Add(relocatedSolution);
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
                    var swappedRoute = swappedSolution.Routes.Single(r => r.Vehicle.Id == route1.Vehicle.Id);
                    swappedRoute.Locations[customer1Index] = customer2;
                    swappedRoute.Locations[customer2Index] = customer1;

                    // Update fitness
                    swappedSolution.CalculateFitness(model.Depots.Count);

                    neighbors.Add(swappedSolution);

                    // Two-opt
                    var twoOptSolution = currentSolution.Clone();
                    var twoOptRoute = twoOptSolution.Routes.Single(r => r.Vehicle.Id == route1.Vehicle.Id);
                    twoOptRoute.Locations.Reverse(Math.Min(customer1Index, customer2Index), Math.Abs(customer1Index - customer2Index) + 1);

                    // Update fitness
                    twoOptSolution.CalculateFitness(model.Depots.Count);

                    neighbors.Add(twoOptSolution);
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
                    var twoOptRoute = twoOptSolution.Routes.Single(r => r.Vehicle.Id == route.Vehicle.Id);

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

