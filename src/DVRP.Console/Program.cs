﻿// Create a DVRP model with your data (depots, customers, and vehicles)
using DVRP.Application.Handlers;
using DVRP.Domain.Entities;

var depots = new List<Depot>
    {
        new Depot { Id = "D1", X = 10, Y = 0 },
        new Depot { Id = "D2",  X = -10, Y = 0 },
    };

DvrpModel model = new()
{
    Depots = depots,
    Customers = new List<Customer>
    {
        new Customer { Id = "C1",  X = 10, Y = 10, Demand = 30 },
        new Customer { Id = "C2",  X = 10, Y = -10, Demand = 30 },
        new Customer { Id = "C3",  X = -10, Y = -10, Demand = 30 },
        new Customer { Id = "C4",  X = -10, Y = 10, Demand = 30 },
        new Customer { Id = "C5",  X = 20, Y = 20, Demand = 30 },
        new Customer { Id = "C6",  X = 20, Y = -20, Demand = 30 },
        new Customer { Id = "C7",  X = -20, Y = -20, Demand = 30 },
        new Customer { Id = "C8",  X = -20, Y = 20, Demand = 30 }
    },
    Vehicles = new List<Vehicle>
    {
        new Vehicle { Id = "V1", Capacity = 60, DepotId = depots[0].Id },
        new Vehicle { Id = "V2", Capacity = 60, DepotId = depots[0].Id },
        new Vehicle { Id = "V3", Capacity = 60, DepotId = depots[1].Id },
        new Vehicle { Id = "V4", Capacity = 60, DepotId = depots[1].Id }
    }
};

// Create GeneticAlgorithmSolver instance
//GeneticAlgorithmSolver solver = new();

//// Define solver parameters
//GeneticAlgorithmParameters parameters = new()
//{
//    PopulationSize = 50,
//    MutationRate = 0.01,
//    CrossoverRate = 0.1,
//    TournamentSize = 3,
//    MaxGenerations = 200,
//    SelectionMethod = DVRP.Domain.Enums.SelectionMethod.RouletteWheelSelection
//};

// Create AntColonyOptimkizationSolver instance
//AntColonyOptimizationSolver solver = new();

//// Define solver parameters
//AntColonyParameters parameters = new()
//{
//    MaxIterations = 100,
//    NumberOfAnts = 10,
//    Alpha = 1,
//    Beta = 2,
//    EvaporationRate = 0.1,
//    Q = 10
//};

//GaAcoSolver solver = new();

//GaAcoParameters parameters = new()
//{
//    GeneticAlgorithmParameters = new()
//    {
//        PopulationSize = 50,
//        MutationRate = 0.01,
//        CrossoverRate = 0.1,
//        TournamentSize = 2,
//        MaxGenerations = 200,
//        SelectionMethod = DVRP.Domain.Enums.GeneticAlgorithmSelectionMethod.RouletteWheel
//    },
//    AntColonyParameters = new()
//    {
//        MaxIterations = 100,
//        NumberOfAnts = 10,
//        Alpha = 1,
//        Beta = 2,
//        EvaporationRate = 0.1,
//        Q = 10
//    }
//};

TabuSearchSolver solver = new();

TabuSearchParameters parameters = new()
{
    MaxIterations = 500,
    TabuListSize = 10,
    NeighborhoodSearchSize = 10,
    DiversificationFactor = 1,
    IntensificationFactor = 1
};

//TsAcoSolver solver = new();

//TsAcoParameters parameters = new()
//{
//    TabuSearchParameters = new()
//    {
//        MaxIterations = 500,
//        TabuListSize = 10,
//        NeighborhoodSearchSize = 10,
//        DiversificationFactor = 1,
//        IntensificationFactor = 1
//    },
//    AntColonyParameters = new()
//    {
//        MaxIterations = 100,
//        NumberOfAnts = 10,
//        Alpha = 1,
//        Beta = 2,
//        EvaporationRate = 0.1,
//        Q = 10
//    }
//};

// Solve the DVRP problem
DvrpSolution solution = solver.Solve(model, parameters);

// Output the results
Console.WriteLine($"Total distance: {solution.TotalDistance:F0}");
Console.WriteLine("Routes:");
foreach (var route in solution.Routes)
{
    Console.WriteLine($"Vehicle {route.Vehicle.Id}: {string.Join(" -> ", route.Locations.Select(l => l.Id))}; Distance: {route.Distance:F2}");
}
