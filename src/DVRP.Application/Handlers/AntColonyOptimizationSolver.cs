using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class AntColonyOptimizationSolver : IDvrpSolver
{
    public Algorithm Algorithm => Algorithm.AntColonyOptimization;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters)
    {
        return Solve(model, parameters);
    }

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not AntColonyParameters antColonyParameters)
        {
            throw new ArgumentException("Invalid parameters type", nameof(parameters));
        }

        int numberOfLocations = model.Customers.Count + model.Depots.Count;
        double[][] pheromoneMatrix = InitializePheromoneMatrix(numberOfLocations);
        double[][] distanceMatrix = CalculateDistanceMatrix(model);

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

            UpdatePheromoneMatrix(pheromoneMatrix, antSolutions, antColonyParameters);
        }

        return bestSolution ?? new DvrpSolution();
    }

    private static double[][] InitializePheromoneMatrix(int numberOfLocations)
    {
        double initialPheromoneValue = 0.01;
        double[][] pheromoneMatrix = new double[numberOfLocations][];

        for (int i = 0; i < numberOfLocations; i++)
        {
            pheromoneMatrix[i] = new double[numberOfLocations];

            for (int j = 0; j < numberOfLocations; j++)
            {
                if (i == j)
                {
                    pheromoneMatrix[i][j] = 0;
                }
                else
                {
                    pheromoneMatrix[i][j] = initialPheromoneValue;
                }
            }
        }

        return pheromoneMatrix;
    }

    private static double[][] CalculateDistanceMatrix(DvrpModel model)
    {
        List<Location> locations = new();
        locations.AddRange(model.Depots);
        locations.AddRange(model.Customers);

        int numberOfLocations = locations.Count;
        double[][] distanceMatrix = new double[numberOfLocations][];

        for (int i = 0; i < numberOfLocations; i++)
        {
            distanceMatrix[i] = new double[numberOfLocations];

            for (int j = 0; j < numberOfLocations; j++)
            {
                if (i == j)
                {
                    distanceMatrix[i][j] = 0;
                }
                else
                {
                    distanceMatrix[i][j] = locations[i].CalculateDistance(locations[j]);
                }
            }
        }

        return distanceMatrix;
    }

    private static DvrpSolution ConstructSolution(DvrpModel model, AntColonyParameters parameters, double[][] pheromoneMatrix, double[][] distanceMatrix)
    {
        List<Customer> unvisitedCustomers = new(model.Customers);
        List<Vehicle> vehicles = new(model.Vehicles);
        List<VehicleRoute> vehicleRoutes = new();

        while (unvisitedCustomers.Count > 0 && vehicles.Count > 0)
        {
            Vehicle vehicle = vehicles[0];
            vehicles.RemoveAt(0);

            VehicleRoute vehicleRoute = new() { Vehicle = vehicle with { } };
            Location currentLocation = model.Depots.First(d => d.Id == vehicle.DepotId);
            double remainingCapacity = vehicle.Capacity;

            while (true)
            {
                Location? nextLocation = SelectNextLocation(currentLocation, unvisitedCustomers, remainingCapacity, pheromoneMatrix, distanceMatrix, parameters);

                if (nextLocation == null)
                {
                    vehicleRoute.Locations.Add(vehicle); // Return to the depot
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
        double[][] pheromoneMatrix, double[][] distanceMatrix, AntColonyParameters parameters)
    {
        List<Customer> feasibleCustomers = unvisitedCustomers.Where(c => c.Demand <= remainingCapacity).ToList();

        if (feasibleCustomers.Count == 0)
        {
            return null; // No more customers can be visited with the remaining capacity
        }

        int currentIndex = currentLocation.Id;
        double sumPheromoneDistance = feasibleCustomers.Sum(c => Math.Pow(pheromoneMatrix[currentIndex][c.Id], parameters.Alpha) * Math.Pow(1 / distanceMatrix[currentIndex][c.Id], parameters.Beta));

        double randomValue = Random.Shared.NextDouble();
        double accumulatedProbability = 0;

        foreach (Customer customer in feasibleCustomers)
        {
            double probability = (Math.Pow(pheromoneMatrix[currentIndex][customer.Id], parameters.Alpha) * Math.Pow(1 / distanceMatrix[currentIndex][customer.Id], parameters.Beta)) / sumPheromoneDistance;
            accumulatedProbability += probability;

            if (randomValue <= accumulatedProbability)
            {
                return customer;
            }
        }

        return feasibleCustomers.Last(); // If no location has been selected yet, choose the last feasible customer
    }

    private static void UpdatePheromoneMatrix(double[][] pheromoneMatrix, List<DvrpSolution> antSolutions, AntColonyParameters parameters)
    {
        int numberOfLocations = pheromoneMatrix.Length;

        // Evaporate pheromones
        for (int i = 0; i < numberOfLocations; i++)
        {
            for (int j = 0; j < numberOfLocations; j++)
            {
                pheromoneMatrix[i][j] *= (1 - parameters.EvaporationRate);
            }
        }

        // Deposit pheromones based on ant solutions
        foreach (DvrpSolution solution in antSolutions)
        {
            double deltaPheromone = parameters.Q / solution.TotalDistance;

            foreach (VehicleRoute route in solution.Routes)
            {
                for (int i = 0; i < route.Locations.Count - 1; i++)
                {
                    int currentLocationId = route.Locations[i].Id;
                    int nextLocationId = route.Locations[i + 1].Id;

                    pheromoneMatrix[currentLocationId][nextLocationId] += deltaPheromone;
                    pheromoneMatrix[nextLocationId][currentLocationId] += deltaPheromone;
                }
            }
        }
    }
}
