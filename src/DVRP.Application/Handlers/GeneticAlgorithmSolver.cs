using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class GeneticAlgorithmSolver : IDvrpSolver
{
    private DvrpModel _model = null!;

    public Algorithm Algorithm => Algorithm.GeneticAlgorithm;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters)
    {
        if (parameters is not GeneticAlgorithmParameters gaParameters)
        {
            throw new ArgumentException("Invalid solver parameters for Genetic Algorithm.");
        }

        if (!IsProblemSolvable(model))
        {
            return new DvrpSolution();
        }

        _model = model;
        return RunGeneticAlgorithm(gaParameters);
    }

    private static bool IsProblemSolvable(DvrpModel model)
    {
        if (model.Customers.Count == 0 || model.Vehicles.Count == 0)
        {
            return false;
        }

        double totalDemand = model.Customers.Sum(customer => customer.Demand);
        double totalVehicleCapacity = model.Vehicles.Sum(vehicle => vehicle.Capacity);

        return totalDemand <= totalVehicleCapacity;
    }

    private DvrpSolution RunGeneticAlgorithm(GeneticAlgorithmParameters parameters)
    {
        var population = InitializePopulation(parameters.PopulationSize);
        for (int generation = 0; generation < parameters.MaxGenerations; generation++)
        {
            population = EvolvePopulation(population, parameters);
        }

        return GetBestSolution(population);
    }

    private List<DvrpSolution> InitializePopulation(int populationSize)
    {
        var population = new List<DvrpSolution>(populationSize);
        for (int i = 0; i < populationSize; i++)
        {
            var solution = GenerateRandomSolution();
            population.Add(solution);
        }
        
        return population;
    }

    private DvrpSolution GenerateRandomSolution()
    {
        var routes = new List<VehicleRoute>();
        var copyCustomers = new List<Customer>(_model.Customers);

        ShuffleList(copyCustomers); // Shuffle the customers list

        var remainingCustomers = new Queue<Customer>(copyCustomers);

        while (remainingCustomers.Count > 0)
        {
            foreach (var vehicle in _model.Vehicles)
            {
                if (remainingCustomers.Count <= 0)
                    break;

                var depot = _model.Depots.First(d => d.Id == vehicle.DepotId);
                Location currentLocation = depot with { };
                var route = new VehicleRoute { VehicleId = vehicle.Id, LocationIds = new List<int>() };
                double remainingCapacity = vehicle.Capacity;

                while (remainingCustomers.Count > 0)
                {
                    var customer = remainingCustomers.Peek();
                    if (customer.Demand > remainingCapacity)
                    {
                        break;
                    }
                    
                    remainingCustomers.Dequeue();
                    route.LocationIds.Add(customer.Id);
                    remainingCapacity -= customer.Demand;
                    route.Distance += CalculateDistance(currentLocation, customer);
                    currentLocation = customer;
                }

                routes.Add(route);
            }
        }

        return new DvrpSolution { Routes = routes };
    }

    private static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Shared.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    private static double CalculateDistance(Location location1, Location location2)
    {
        double dx = location1.X - location2.X;
        double dy = location1.Y - location2.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private List<DvrpSolution> EvolvePopulation(List<DvrpSolution> population, GeneticAlgorithmParameters parameters)
    {
        var selectedParents = SelectParents(population, parameters);
        var offspring = PerformCrossover(selectedParents, parameters.CrossoverRate);
        MutateOffspring(offspring, parameters.MutationRate);
        return ApplyElitism(population, offspring);
    }

    private static List<DvrpSolution> SelectParents(List<DvrpSolution> population, GeneticAlgorithmParameters parameters)
    {
        return parameters.SelectionMethod switch
        {
            GeneticAlgorithmSelectionMethod.RouletteWheel => RouletteWheelSelection(population),
            GeneticAlgorithmSelectionMethod.Tournament => TournamentSelection(population, parameters.TournamentSize),
            _ => throw new ArgumentException("Invalid selection method."),
        };
    }

    private static List<DvrpSolution> RouletteWheelSelection(List<DvrpSolution> population)
    {
        var selectedParents = new List<DvrpSolution>(population.Count);

        var fitnesses = population.Select(p => 1 / p.TotalDistance);
        double fitnessSum = fitnesses.Sum();
        var normalizedFitnesses = fitnesses.Select(f => f / fitnessSum).ToList();
        var cumulativeProbabilities = CalculateCumulativeProbabilities(normalizedFitnesses);

        for (int i = 0; i < population.Count; i++)
        {
            double randomValue = Random.Shared.NextDouble();
            int selectedIndex = FindIndexByProbability(cumulativeProbabilities, randomValue);
            selectedParents.Add(population[selectedIndex]);
        }

        return selectedParents;
    }

    private static List<double> CalculateCumulativeProbabilities(List<double> probabilities)
    {
        var cumulativeProbabilities = new List<double>(probabilities.Count);
        double cumulativeProbability = 0;

        foreach (var probability in probabilities)
        {
            cumulativeProbability += probability;
            cumulativeProbabilities.Add(cumulativeProbability);
        }

        return cumulativeProbabilities;
    }

    private static int FindIndexByProbability(List<double> cumulativeProbabilities, double randomValue)
    {
        for (int i = 0; i < cumulativeProbabilities.Count; i++)
        {
            if (randomValue <= cumulativeProbabilities[i])
            {
                return i;
            }
        }

        return cumulativeProbabilities.Count - 1;
    }

    private static List<DvrpSolution> TournamentSelection(List<DvrpSolution> population, int tournamentSize = 2)
    {
        var selectedParents = new List<DvrpSolution>(population.Count);

        for (int i = 0; i < population.Count; i++)
        {
            var competitors = new List<DvrpSolution>(tournamentSize);
            for (int j = 0; j < tournamentSize; j++)
            {
                int randomIndex = Random.Shared.Next(population.Count);
                competitors.Add(population[randomIndex]);
            }

            var bestCompetitor = competitors.OrderBy(solution => solution.TotalDistance).First();
            selectedParents.Add(bestCompetitor);
        }

        return selectedParents;
    }

    private List<DvrpSolution> PerformCrossover(List<DvrpSolution> parents, double crossoverRate)
    {
        var offspring = new List<DvrpSolution>(parents.Count);

        for (int i = 0; i < parents.Count; i += 2)
        {
            if (i + 1 >= parents.Count)
            {
                break;
            }

            if (Random.Shared.NextDouble() < crossoverRate)
            {
                var child1 = OrderedCrossover(parents[i], parents[i + 1]);
                var child2 = OrderedCrossover(parents[i + 1], parents[i]);

                offspring.Add(child1);
                offspring.Add(child2);
            }
            else
            {
                offspring.Add(parents[i]);
                offspring.Add(parents[i + 1]);
            }
        }

        return offspring;
    }

    private DvrpSolution OrderedCrossover(DvrpSolution parent1, DvrpSolution parent2)
    {
        var childRoutes = new List<VehicleRoute>(parent1.Routes.Count);

        for (int i = 0; i < parent1.Routes.Count; i++)
        {
            var parent1Route = parent1.Routes[i].LocationIds;
            var parent2Route = parent2.Routes[i].LocationIds;

            int crossoverStart = Random.Shared.Next(parent1Route.Count);
            int crossoverEnd = Random.Shared.Next(crossoverStart, parent1Route.Count);

            var childRoute = new VehicleRoute
            {
                VehicleId = parent1.Routes[i].VehicleId,
                LocationIds = new List<int>(parent1Route.Count)
            };

            childRoute.LocationIds.AddRange(parent1Route.GetRange(crossoverStart, crossoverEnd - crossoverStart));

            foreach (int locationId in parent2Route)
            {
                if (!childRoute.LocationIds.Contains(locationId))
                {
                    childRoute.LocationIds.Add(locationId);
                }
            }

            childRoute.Distance += CalculateRouteDistance(childRoute, parent1.Routes[i].VehicleId);
            childRoutes.Add(childRoute);
        }

        return new DvrpSolution { Routes = childRoutes };
    }

    private double CalculateRouteDistance(VehicleRoute route, int vehicleId)
    {
        double distance = 0;
        var vehicle = _model.Vehicles.First(v => v.Id == vehicleId);
        var depot = _model.Depots.First(d => d.Id == vehicle.DepotId);

        if (route.LocationIds.Count > 0)
        {
            var firstCustomer = _model.Customers.First(c => c.Id == route.LocationIds[0]);
            distance += CalculateDistance(depot, firstCustomer);

            for (int i = 0; i < route.LocationIds.Count - 1; i++)
            {
                var location1 = _model.Customers.First(c => c.Id == route.LocationIds[i]);
                var location2 = _model.Customers.First(c => c.Id == route.LocationIds[i + 1]);
                distance += CalculateDistance(location1, location2);
            }

            var lastCustomer = _model.Customers.First(c => c.Id == route.LocationIds.Last());
            distance += CalculateDistance(lastCustomer, depot);
        }

        return distance;
    }

    private void MutateOffspring(List<DvrpSolution> offspring, double mutationRate)
    {
        foreach (var solution in offspring)
        {
            if (Random.Shared.NextDouble() < mutationRate)
            {
                int routeIndex = Random.Shared.Next(solution.Routes.Count);
                var route = solution.Routes[routeIndex];

                if (route.LocationIds.Count >= 2)
                {
                    int index1 = Random.Shared.Next(route.LocationIds.Count);
                    int index2 = Random.Shared.Next(route.LocationIds.Count);

                    // Swap the customers at the selected indices
                    (route.LocationIds[index2], route.LocationIds[index1]) = (route.LocationIds[index1], route.LocationIds[index2]);
                }
            }
        }
    }

    private static List<DvrpSolution> ApplyElitism(List<DvrpSolution> population, List<DvrpSolution> offspring)
    {
        // Determine the number of elite individuals to preserve
        int eliteCount = (int)(population.Count * 0.1); // Preserve the top 10% of the population; adjust as needed

        // Sort the population and offspring by total distance (ascending)
        var sortedPopulation = population.OrderBy(solution => solution.TotalDistance).ToList();
        var sortedOffspring = offspring.OrderBy(solution => solution.TotalDistance).ToList();

        // Copy the elite individuals from the population
        var nextGeneration = sortedPopulation.Take(eliteCount).ToList();

        // Fill the remaining slots in the next generation with the best offspring
        int remainingCount = population.Count - eliteCount;
        nextGeneration.AddRange(sortedOffspring.Take(remainingCount));

        return nextGeneration;
    }

    private DvrpSolution GetBestSolution(List<DvrpSolution> population)
    {
        return population.OrderBy(s => s.TotalDistance).First();
    }
}
