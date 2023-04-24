// Create a DVRP model with your data (depots, customers, and vehicles)
using DVRP.Application.Handlers;
using DVRP.Domain.Entities;

DvrpModel model = new DvrpModel
{
    Depots = new List<Depot>
    {
        new Depot { Id = 1, X = 0, Y = 0, StartTime = 0, EndTime = 3600 }
    },
    Customers = new List<Customer>
    {
        new Customer { Id = 1, X = 10, Y = 10, Demand = 30, ServiceTime = 3, TimeWindowStart = 0, TimeWindowEnd = 60 },
        new Customer { Id = 2, X = 20, Y = 20, Demand = 30, ServiceTime = 6, TimeWindowStart = 61, TimeWindowEnd = 120 },
        new Customer { Id = 3, X = -10, Y = -10, Demand = 30, ServiceTime = 9, TimeWindowStart = 0, TimeWindowEnd = 120 },
        new Customer { Id = 4, X = -20, Y = -20, Demand = 30, ServiceTime = 12, TimeWindowStart = 121, TimeWindowEnd = 240 },
    },
    Vehicles = new List<Vehicle>
    {
        new Vehicle { Id = 1, Capacity = 60, DepotId = 1 },
        new Vehicle { Id = 2, Capacity = 60, DepotId = 1 }
    }
};

// Create GeneticAlgorithmSolver instance
GeneticAlgorithmSolver solver = new GeneticAlgorithmSolver();

// Define solver parameters
GeneticAlgorithmParameters parameters = new GeneticAlgorithmParameters
{
    PopulationSize = 100,
    MaxGenerations = 100,
    MutationRate = 0.4,
    CrossoverRate = 0.4,
    TournamentSize = 2,
    SelectionMethod = DVRP.Domain.Enums.GeneticAlgorithmSelectionMethod.Tournament
};

// Solve the DVRP problem
DvrpSolution solution = solver.Solve(model, parameters);

// Output the results
Console.WriteLine("Total distance: " + solution.TotalDistance);
Console.WriteLine("Routes:");
foreach (var route in solution.Routes)
{
    Console.WriteLine($"Vehicle {route.VehicleId}: {string.Join(" -> ", route.LocationIds)}");
}
