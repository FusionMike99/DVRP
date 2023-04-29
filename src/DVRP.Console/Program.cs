// Create a DVRP model with your data (depots, customers, and vehicles)
using DVRP.Application.Handlers;
using DVRP.Domain.Entities;

DvrpModel model = new DvrpModel
{
    Depots = new List<Depot>
    {
        new Depot { Id = 1, X = 10, Y = 0 },
        new Depot { Id = 2, X = -10, Y = 0 },
    },
    Customers = new List<Customer>
    {
        new Customer { Id = 3, X = 10, Y = 10, Demand = 30 },
        new Customer { Id = 4, X = 10, Y = -10, Demand = 30 },
        new Customer { Id = 5, X = -10, Y = -10, Demand = 30 },
        new Customer { Id = 6, X = -10, Y = 10, Demand = 30 },
        new Customer { Id = 7, X = 20, Y = 20, Demand = 30 },
        new Customer { Id = 8, X = 20, Y = -20, Demand = 30 },
        new Customer { Id = 9, X = -20, Y = -20, Demand = 30 },
        new Customer { Id = 10, X = -20, Y = 20, Demand = 30 }
    },
    Vehicles = new List<Vehicle>
    {
        new Vehicle { Id = 1, Capacity = 60, DepotId = 1 },
        new Vehicle { Id = 2, Capacity = 60, DepotId = 1 },
        new Vehicle { Id = 3, Capacity = 60, DepotId = 2 },
        new Vehicle { Id = 4, Capacity = 60, DepotId = 2 }
    }
};

// Create GeneticAlgorithmSolver instance
GeneticAlgorithmSolver solver = new GeneticAlgorithmSolver();

// Define solver parameters
GeneticAlgorithmParameters parameters = new GeneticAlgorithmParameters
{
    PopulationSize = 50,
    MutationRate = 0.01,
    CrossoverRate = 0.9,
    TournamentSize = 2,
    MaxGenerations = 200,
    SelectionMethod = DVRP.Domain.Enums.GeneticAlgorithmSelectionMethod.RouletteWheel
};

// Solve the DVRP problem
DvrpSolution solution = solver.Solve(model, parameters);

// Output the results
Console.WriteLine("Total distance: " + solution.TotalDistance);
Console.WriteLine("Routes:");
foreach (var route in solution.Routes)
{
    Console.WriteLine($"Vehicle {route.VehicleId}: {string.Join(" -> ", route.LocationIds)}; Distance: {route.Distance}");
}
