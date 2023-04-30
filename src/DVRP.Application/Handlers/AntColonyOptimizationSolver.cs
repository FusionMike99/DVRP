using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class AntColonyOptimizationSolver : IDvrpSolver
{
    public Algorithm Algorithm => Algorithm.AntColonyOptimization;

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
        List<Customer> unvisitedCustomers = new(model.Customers);
        List<Vehicle> vehicles = new(model.Vehicles);
        List<VehicleRoute> vehicleRoutes = new();

        while (unvisitedCustomers.Count > 0 && vehicles.Count > 0)
        {
            Vehicle vehicle = vehicles[0];
            vehicles.RemoveAt(0);
            
            Location depot = model.Depots.First(d => d.Id == vehicle.DepotId);
            VehicleRoute vehicleRoute = new() { Vehicle = vehicle with { }, Locations = new() { depot with { } } };
            Location currentLocation = depot with { };
            double remainingCapacity = vehicle.Capacity;

            while (true)
            {
                Location? nextLocation = SelectNextLocation(currentLocation, unvisitedCustomers, remainingCapacity, pheromoneMatrix, distanceMatrix, parameters);

                if (nextLocation == null)
                {
                    vehicleRoute.Locations.Add(depot); // Return to the depot
                    break;
                }

                vehicleRoute.Locations.Add(nextLocation);
                currentLocation = nextLocation;

                if (nextLocation is Customer customer)
                {
                    remainingCapacity -= customer.Demand;
                    unvisitedCustomers.Remove(customer);
                }
            }

            vehicleRoutes.Add(vehicleRoute);
        }

        DvrpSolution solution = new() { Routes = vehicleRoutes };
        return solution;
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
