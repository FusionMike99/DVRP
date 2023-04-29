namespace DVRP.Domain.Entities;

public class Ant
{
    private readonly DvrpModel _model;
    private readonly DistanceMatrix _distanceMatrix;
    private readonly PheromoneMatrix _pheromoneMatrix;

    public Ant(DvrpModel model, DistanceMatrix distanceMatrix, PheromoneMatrix pheromoneMatrix)
    {
        _model = model;
        _distanceMatrix = distanceMatrix;
        _pheromoneMatrix = pheromoneMatrix;
    }

    public DvrpSolution BuildSolution(double alpha, double beta, int currentVehicle)
    {
        int numberOfDepots = _model.Depots.Count;
        int numberOfCustomers = _model.Customers.Count;
        int numberOfLocations = numberOfDepots + numberOfCustomers;

        HashSet<int> unvisitedCustomers = new(Enumerable.Range(numberOfDepots, numberOfCustomers)); // All customers
        var firstDepot = _model.Depots[0] with { };
        List<Location> solution = new() { _model.Depots[0] }; // Start at the first depot (index 0)

        int currentLocation = 0;
        double currentVehicleCapacity = _model.Vehicles[currentVehicle].Capacity;

        while (unvisitedCustomers.Count > 0)
        {
            double[] probabilities = new double[numberOfLocations];
            double totalProbability = 0.0;

            // Calculate the probabilities for each unvisited customer
            foreach (int customerIndex in unvisitedCustomers)
            {
                double pheromoneTrail = Math.Pow(_pheromoneMatrix.GetPheromoneTrail(currentLocation, customerIndex), alpha);
                double distanceInfo = Math.Pow(1.0 / _distanceMatrix.GetDistance(currentLocation, customerIndex), beta);

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
                solution.Add(firstDepot with { }); // Return to the first depot (index 0)
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
        List<Location> currentRoute = new();
        double currentRouteDistance = 0;
        for (int i = 1; i < solution.Count; i++)
        {
            int locationIndex = solution[i];
            currentRoute.Add(locationIndex);

            if (locationIndex < numberOfDepots) // Reached the depot
            {
                VehicleRoute vehicleRoute = new()
                {
                    Vehicle = _model.Vehicles.First(v => v.Id == currentVehicle),
                    Locations = currentRoute
                };

                vehicleRoutes.Add(vehicleRoute);
                currentRoute = new();
                currentRouteDistance = 0;
            }
            else // Reached a customer
            {
                currentRouteDistance += _distanceMatrix.GetDistance(solution[i - 1].Id, locationIndex);
            }
        }

        // Create the DvrpSolution instance
        DvrpSolution antSolution = new()
        {
            Routes = vehicleRoutes
        };

        return antSolution;
    }

    private static int ChooseNextCustomer(double[] probabilities)
    {
        double randomValue = Random.Shared.NextDouble();
        double cumulativeProbability = 0.0;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        // In case of rounding errors, return the last customer
        return probabilities.Length - 1;
    }

    private double CalculateRouteDistance(List<int> solution)
    {
        double totalDistance = 0.0;

        for (int i = 1; i < solution.Count; i++)
        {
            totalDistance += _distanceMatrix.GetDistance(solution[i - 1], solution[i]);
        }

        return totalDistance;
    }
}
