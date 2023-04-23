using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class AntColonyOptimizationSolver : IDvrpSolver
{
    private DvrpModel _model = null!;
    private double[][] _distances = null!;
    private double[][] _pheromoneTrails = null!;

    public Algorithm Algorithm => Algorithm.AntColonyOptimization;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not AntColonyParameters antColonyParameters)
        {
            throw new ArgumentException("The provided parameters must be of type AntColonyParameters.");
        }

        Initialize(model);

        int numberOfAnts = antColonyParameters.NumberOfAnts;
        int numberOfIterations = antColonyParameters.MaxIterations;
        double alpha = antColonyParameters.Alpha;
        double beta = antColonyParameters.Beta;
        double rho = antColonyParameters.EvaporationRate;
        double q = antColonyParameters.Q; // You can add this as a property to AntColonyParameters if needed

        DvrpSolution? bestSolution = initialSolution;

        int currentVehicle = 0;

        for (int iteration = 0; iteration < numberOfIterations; iteration++)
        {
            List<DvrpSolution> antSolutions = new(numberOfAnts);

            for (int ant = 0; ant < numberOfAnts; ant++)
            {
                DvrpSolution antSolution = BuildAntSolution(alpha, beta, currentVehicle);
                antSolutions.Add(antSolution);

                if (bestSolution is null || antSolution.TotalDistance < bestSolution.TotalDistance)
                {
                    bestSolution = antSolution;
                }
            }

            UpdatePheromoneTrails(antSolutions, rho, q);

            // Update the current vehicle index
            currentVehicle = (currentVehicle + 1) % _model.Vehicles.Count;
        }

        return bestSolution!;
    }


    // Additional methods and helpers for ACO
    private void Initialize(DvrpModel model)
    {
        _model = model;
        int numberOfCustomers = model.Customers.Count;
        int numberOfDepots = model.Depots.Count;
        int numberOfLocations = numberOfCustomers + numberOfDepots;

        // Initialize distance matrix
        _distances = new double[numberOfLocations][];
        for (int i = 0; i < numberOfLocations; i++)
        {
            _distances[i] = new double[numberOfLocations];
            for (int j = 0; j < numberOfLocations; j++)
            {
                Location location1 = i < numberOfDepots ? model.Depots[i] : model.Customers[i - numberOfDepots];
                Location location2 = j < numberOfDepots ? model.Depots[j] : model.Customers[j - numberOfDepots];
                _distances[i][j] = CalculateDistance(location1, location2);
            }
        }

        // Initialize pheromone trails
        _pheromoneTrails = new double[numberOfLocations][];
        for (int i = 0; i < numberOfLocations; i++)
        {
            _pheromoneTrails[i] = new double[numberOfLocations];
            for (int j = 0; j < numberOfLocations; j++)
            {
                _pheromoneTrails[i][j] = 1.0;
            }
        }
    }

    private static double CalculateDistance(Location location1, Location location2)
    {
        double deltaX = location1.X - location2.X;
        double deltaY = location1.Y - location2.Y;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    private DvrpSolution BuildAntSolution(double alpha, double beta, int currentVehicle)
    {
        int numberOfDepots = _model.Depots.Count;
        int numberOfCustomers = _model.Customers.Count;
        int numberOfLocations = numberOfDepots + numberOfCustomers;

        HashSet<int> unvisitedCustomers = new(Enumerable.Range(numberOfDepots, numberOfCustomers)); // All customers
        List<int> solution = new() { 0 }; // Start at the first depot (index 0)

        int currentLocation = 0;
        double currentVehicleCapacity = _model.Vehicles[currentVehicle].Capacity;

        while (unvisitedCustomers.Count > 0)
        {
            double[] probabilities = new double[numberOfLocations];
            double totalProbability = 0.0;

            // Calculate the probabilities for each unvisited customer
            foreach (int customerIndex in unvisitedCustomers)
            {
                double pheromoneTrail = Math.Pow(_pheromoneTrails[currentLocation][customerIndex], alpha);
                double distanceInfo = Math.Pow(1.0 / _distances[currentLocation][customerIndex], beta);

                double probability = pheromoneTrail * distanceInfo;
                probabilities[customerIndex] = probability;
                totalProbability += probability;
            }

            // Normalize the probabilities
            for (int i = numberOfDepots; i < numberOfLocations; i++)
            {
                if (unvisitedCustomers.Contains(i))
                {
                    probabilities[i] /= totalProbability;
                }
                else
                {
                    probabilities[i] = 0.0;
                }
            }

            // Choose the next customer based on the probabilities
            int nextCustomer = ChooseNextCustomer(probabilities);

            // Check if the current vehicle can serve the next customer, otherwise return to the depot
            if (_model.Customers[nextCustomer - numberOfDepots].Demand > currentVehicleCapacity)
            {
                solution.Add(0); // Return to the first depot (index 0)
                currentVehicleCapacity = _model.Vehicles[currentVehicle].Capacity; // Reset the vehicle capacity
            }

            // Visit the next customer
            solution.Add(nextCustomer);
            unvisitedCustomers.Remove(nextCustomer);
            currentVehicleCapacity -= _model.Customers[nextCustomer - numberOfDepots].Demand;
            currentLocation = nextCustomer;
        }

        // Return to the depot at the end of the route
        solution.Add(0);

        // Calculate the total distance of the solution
        double totalDistance = CalculateRouteDistance(solution);

        // Create VehicleRoute instances from the solution
        List<VehicleRoute> vehicleRoutes = new();
        List<int> currentRoute = new();
        double currentRouteDistance = 0;
        for (int i = 1; i < solution.Count; i++)
        {
            int locationIndex = solution[i];
            currentRoute.Add(locationIndex);

            if (locationIndex < numberOfDepots) // Reached the depot
            {
                VehicleRoute vehicleRoute = new()
                {
                    VehicleId = currentVehicle,
                    LocationIds = new List<int>(currentRoute),
                    Distance = currentRouteDistance
                };

                vehicleRoutes.Add(vehicleRoute);
                currentRoute.Clear();
                currentRouteDistance = 0;
            }
            else
            {
                currentRouteDistance += _distances[currentRoute[^2]][locationIndex];
            }
        }

        return new DvrpSolution(vehicleRoutes, totalDistance);
    }

    private static int ChooseNextCustomer(double[] probabilities)
    {
        double randomValue = Random.Shared.NextDouble();
        double cumulativeProbability = 0.0;

        for (int i = 1; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        // In case the cumulative probability is slightly less than 1.0 due to floating-point inaccuracies
        return probabilities.Length - 1;
    }

    private double CalculateRouteDistance(List<int> solution)
    {
        double totalDistance = 0;

        for (int i = 0; i < solution.Count - 1; i++)
        {
            int location1 = solution[i];
            int location2 = solution[i + 1];
            totalDistance += _distances[location1][location2];
        }

        return totalDistance;
    }

    private void UpdatePheromoneTrails(List<DvrpSolution> antSolutions, double rho, double q)
    {
        // Evaporate pheromone trails
        for (int i = 0; i < _pheromoneTrails.Length; i++)
        {
            for (int j = 0; j < _pheromoneTrails[i].Length; j++)
            {
                _pheromoneTrails[i][j] *= (1 - rho);
            }
        }

        // Deposit pheromones based on the solutions
        foreach (DvrpSolution antSolution in antSolutions)
        {
            double pheromoneDeposit = q / antSolution.TotalDistance;

            foreach (VehicleRoute vehicleRoute in antSolution.Routes)
            {
                List<int> locationIds = vehicleRoute.LocationIds;
                for (int i = 0; i < locationIds.Count - 1; i++)
                {
                    int from = locationIds[i];
                    int to = locationIds[i + 1];
                    _pheromoneTrails[from][to] += pheromoneDeposit;
                    _pheromoneTrails[to][from] += pheromoneDeposit; // Assuming symmetric pheromone trails
                }
            }
        }
    }

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters)
    {
        throw new NotImplementedException();
    }
}
