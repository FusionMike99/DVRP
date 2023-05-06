using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class AntColonyOptimizationSolver : IDvrpSolver
{
    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not AntColonyParameters antColonyParameters)
        {
            throw new ArgumentException("Invalid parameters type", nameof(parameters));
        }

        PheromoneMatrix pheromoneMatrix = new(model);
        DistanceMatrix distanceMatrix = new(model);

        DvrpSolution? bestSolution = initialSolution;
        double bestFitness = double.MaxValue;

        for (int iteration = 0; iteration < antColonyParameters.MaxIterations; iteration++)
        {
            List<DvrpSolution> antSolutions = new();

            for (int ant = 0; ant < antColonyParameters.NumberOfAnts; ant++)
            {
                DvrpSolution antSolution = ConstructSolution(model, antColonyParameters, pheromoneMatrix, distanceMatrix);
                antSolution.CalculateFitness(model.Depots.Count);

                if (antSolution.Fitness < bestFitness)
                {
                    bestSolution = antSolution;
                    bestFitness = antSolution.Fitness;
                }

                antSolutions.Add(antSolution);
            }

            pheromoneMatrix.UpdateData(antSolutions, antColonyParameters);
        }

        return bestSolution ?? new DvrpSolution();
    }

    private static DvrpSolution ConstructSolution(DvrpModel model, AntColonyParameters parameters, PheromoneMatrix pheromoneMatrix, DistanceMatrix distanceMatrix)
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
                    Location? nextLocation = SelectNextLocation(currentLocation, remainingCustomers, currentCapacity, pheromoneMatrix, distanceMatrix, parameters);

                    if (nextLocation == null)
                    {
                        break;
                    }

                    route.Locations.Add(nextLocation);
                    currentLocation = nextLocation;

                    if (nextLocation is Customer customer)
                    {
                        currentCapacity -= customer.Demand;
                        remainingCustomers.Remove(customer);
                    }
                }

                route.Locations.Add(depot);  // Return to the depot
                routes.Add(route);
            }
        }

        return new DvrpSolution { Routes = routes };
    }

    private static Location? SelectNextLocation(Location currentLocation, List<Customer> unvisitedCustomers, double remainingCapacity,
        PheromoneMatrix pheromoneMatrix, DistanceMatrix distanceMatrix, AntColonyParameters parameters)
    {
        List<Customer> feasibleCustomers = unvisitedCustomers.Where(c => c.Demand <= remainingCapacity).ToList();

        if (feasibleCustomers.Count == 0)
        {
            return null; // No more customers can be visited with the remaining capacity
        }

        var currentIndex = currentLocation.Id;
        double sumPheromoneDistance = feasibleCustomers.Sum(c => Math.Pow(pheromoneMatrix[currentIndex, c.Id], parameters.Alpha) * Math.Pow(1 / distanceMatrix[currentIndex, c.Id], parameters.Beta));

        double randomValue = Random.Shared.NextDouble();
        double accumulatedProbability = 0;

        foreach (Customer customer in feasibleCustomers)
        {
            double probability = Math.Pow(pheromoneMatrix[currentIndex, customer.Id], parameters.Alpha) * Math.Pow(1 / distanceMatrix[currentIndex, customer.Id], parameters.Beta) / sumPheromoneDistance;
            accumulatedProbability += probability;

            if (randomValue <= accumulatedProbability)
            {
                return customer;
            }
        }

        return feasibleCustomers.Last(); // If no location has been selected yet, choose the last feasible customer
    }
}
