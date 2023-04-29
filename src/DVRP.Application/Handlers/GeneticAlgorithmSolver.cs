using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class GeneticAlgorithmSolver : IDvrpSolver
{
    public Algorithm Algorithm => Algorithm.GeneticAlgorithm;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters)
    {
        if(parameters is not GeneticAlgorithmParameters gaParameters)
        {
            throw new ArgumentException("Not suitable parameter", nameof(parameters));
        }

        // 1. Initialize the population
        List<DvrpSolution> population = InitializePopulation(model, gaParameters);

        // 2. Evaluate the initial population
        EvaluatePopulation(model, population);

        // 3. Main loop
        for (int generation = 0; generation < gaParameters.MaxGenerations; generation++)
        {
            // 3.1 Selection
            var selectedParents = SelectParents(population, gaParameters);

            // 3.2 Crossover
            var offspring = PerformCrossover(model, selectedParents, gaParameters);

            // 3.3 Mutation
            PerformMutation(model, offspring, gaParameters);

            // 3.4 Replacement
            ReplacePopulation(population, offspring);

            // 3.5 Evaluate the population
            EvaluatePopulation(model, population);
        }

        // 4. Get the best solution from the final population
        var bestSolution = GetBestSolution(population);
        return bestSolution;
    }

    private static List<DvrpSolution> InitializePopulation(DvrpModel model, GeneticAlgorithmParameters gaParameters)
    {
        List<DvrpSolution> population = new(gaParameters.PopulationSize);

        for (int i = 0; i < gaParameters.PopulationSize; i++)
        {
            DvrpSolution solution = new();
            List<Customer> remainingCustomers = new(model.Customers.OrderBy(c => Random.Shared.Next()));

            while (remainingCustomers.Count > 0)
            {
                var customerAssigned = false;

                foreach (Vehicle vehicle in model.Vehicles)
                {
                    VehicleRoute route = new() { Vehicle = vehicle with { } };
                    double remainingCapacity = vehicle.Capacity;

                    // Start at the vehicle's depot
                    Depot startDepot = model.Depots.First(depot => depot.Id == vehicle.DepotId) with { };
                    route.Locations.Add(startDepot);

                    // Assign customers to the vehicle while considering capacity constraints
                    for (int j = 0; j < remainingCustomers.Count; j++)
                    {
                        Customer customer = remainingCustomers[j] with { };
                        if (customer.Demand > remainingCapacity)
                            continue;

                        route.Locations.Add(customer);
                        remainingCapacity -= customer.Demand;
                        remainingCustomers.RemoveAt(j);
                        j--; // Decrement the index to account for the removed customer
                        customerAssigned = true;
                    }

                    // Return to the depot at the end of the route
                    route.Locations.Add(startDepot);

                    // Add the route to the solution
                    solution.Routes.Add(route);
                }

                if (!customerAssigned) break;
            }

            // Add the solution to the population
            population.Add(solution);
        }

        return population;
    }

    private static void EvaluatePopulation(DvrpModel model, List<DvrpSolution> population)
    {
        foreach (DvrpSolution solution in population)
        {
            solution.CalculateFitness(model.Depots.Count);
        }
    }

    private static List<DvrpSolution> SelectParents(List<DvrpSolution> population, GeneticAlgorithmParameters gaParameters)
    {
        List<DvrpSolution> parents = new(population.Count);
        Func<DvrpSolution> selectionMethod = gaParameters.SelectionMethod switch
        {
            GeneticAlgorithmSelectionMethod.RouletteWheel => () => RouletteWheelSelection(population),
            GeneticAlgorithmSelectionMethod.Tournament => () => TournamentSelection(population, gaParameters.TournamentSize),
            _ => () => RouletteWheelSelection(population)
        };

        for (int i = 0; i < population.Count; i++)
        {
            parents.Add(selectionMethod());
        }

        return parents;
    }

    private static DvrpSolution RouletteWheelSelection(List<DvrpSolution> population)
    {
        double totalFitness = population.Sum(solution => solution.Fitness);
        double randomValue = Random.Shared.NextDouble() * totalFitness;
        double accumulatedFitness = 0;

        foreach (DvrpSolution solution in population)
        {
            accumulatedFitness += solution.Fitness;
            if (accumulatedFitness >= randomValue)
            {
                return solution;
            }
        }

        return population[^1]; // Return the last solution as a fallback
    }

    private static DvrpSolution TournamentSelection(List<DvrpSolution> population, int tournamentSize)
    {
        List<DvrpSolution> selectedSolutions = new(tournamentSize);
        for (int i = 0; i < tournamentSize; i++)
        {
            int randomIndex = Random.Shared.Next(population.Count);
            selectedSolutions.Add(population[randomIndex]);
        }

        return selectedSolutions.OrderBy(solution => solution.Fitness).First();
    }

    private static List<DvrpSolution> PerformCrossover(DvrpModel model, List<DvrpSolution> selectedParents, GeneticAlgorithmParameters gaParameters)
    {
        List<DvrpSolution> offspringPopulation = new(selectedParents.Count);

        for (int i = 0; i < selectedParents.Count; i += 2)
        {
            DvrpSolution parent1 = selectedParents[i];
            DvrpSolution parent2 = selectedParents[i + 1];

            if (Random.Shared.NextDouble() < gaParameters.CrossoverRate)
            {
                (DvrpSolution offspring1, DvrpSolution offspring2) = OrderCrossover(model, parent1, parent2);
                offspringPopulation.Add(offspring1);
                offspringPopulation.Add(offspring2);
            }
            else
            {
                offspringPopulation.Add(parent1);
                offspringPopulation.Add(parent2);
            }
        }

        return offspringPopulation;
    }

    private static (DvrpSolution, DvrpSolution) OrderCrossover(DvrpModel model, DvrpSolution parent1, DvrpSolution parent2)
    {
        DvrpSolution offspring1 = new();
        DvrpSolution offspring2 = new();

        int numVehicles = model.Vehicles.Count;
        for (int vehicleIndex = 0; vehicleIndex < numVehicles; vehicleIndex++)
        {
            Vehicle vehicle = model.Vehicles[vehicleIndex];
            VehicleRoute parent1Route = parent1.Routes[vehicleIndex];
            VehicleRoute parent2Route = parent2.Routes[vehicleIndex];

            // Remove depots from parent routes.
            List<Customer> parent1Customers = parent1Route.Locations.OfType<Customer>().ToList();
            List<Customer> parent2Customers = parent2Route.Locations.OfType<Customer>().ToList();

            int customerCount = parent1Customers.Count;
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
            VehicleRoute offspring1Route = CreateVehicleRoute(vehicle, model.Depots, offspring1Customers);
            VehicleRoute offspring2Route = CreateVehicleRoute(vehicle, model.Depots, offspring2Customers);

            // Add the routes to the offspring solutions.
            offspring1.Routes.Add(offspring1Route);
            offspring2.Routes.Add(offspring2Route);
        }

        return (offspring1, offspring2);
    }

    private static void FillRemainingCustomers(List<Customer> offspringCustomers, List<Customer> parentCustomers)
    {
        int insertionIndex = 0;
        foreach (Customer customer in parentCustomers)
        {
            if (!offspringCustomers.Contains(customer))
            {
                offspringCustomers.Insert(insertionIndex++, customer);
            }
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

    private static void PerformMutation(DvrpModel model, List<DvrpSolution> offspring, GeneticAlgorithmParameters gaParameters)
    {
        foreach (DvrpSolution solution in offspring)
        {
            foreach (VehicleRoute route in solution.Routes)
            {
                if (Random.Shared.NextDouble() <= gaParameters.MutationRate)
                {
                    List<Customer> customers = route.Locations.OfType<Customer>().ToList();
                    int customerCount = customers.Count;

                    int mutationIndex1 = Random.Shared.Next(customerCount);
                    int mutationIndex2 = Random.Shared.Next(customerCount);

                    int start = Math.Min(mutationIndex1, mutationIndex2);
                    int end = Math.Max(mutationIndex1, mutationIndex2);

                    customers.Reverse(start, end - start);

                    // Repair the solution if capacity constraints are violated
                    // ... implement a repair strategy if needed

                    // Reconstruct the mutated route
                    Depot startDepot = model.Depots.First(depot => depot.Id == route.Vehicle.DepotId) with { };
                    List<Location> mutatedLocations = new() { startDepot };
                    mutatedLocations.AddRange(customers);
                    mutatedLocations.Add(startDepot);
                    route.Locations = mutatedLocations;
                }
            }
        }
    }

    private static void ReplacePopulation(List<DvrpSolution> population, List<DvrpSolution> offspring)
    {
        int elitismSize = (int)(population.Count * 0.1); // Select the top 10% of the solutions

        // Select the best solutions from the current population
        List<DvrpSolution> eliteSolutions = population
            .OrderBy(solution => solution.Fitness)
            .Take(elitismSize)
            .ToList();

        // Remove the worst solutions from the offspring
        offspring = offspring
            .OrderByDescending(solution => solution.Fitness)
            .Skip(elitismSize)
            .ToList();

        // Add the elite solutions to the offspring
        offspring.AddRange(eliteSolutions);

        // Replace the current population with the new offspring solutions
        population.Clear();
        population.AddRange(offspring);
    }

    private static DvrpSolution GetBestSolution(List<DvrpSolution> population)
    {
        // Find the solution with the lowest fitness value
        DvrpSolution bestSolution = population
            .OrderBy(solution => solution.Fitness)
            .First();

        return bestSolution;
    }
}
