using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Pages;

public partial class Index
{
    [Inject]
    private DvrpSolverSelection SolverSelection { get; set; } = null!;

    private Algorithm SelectedAlgorithm { get; set; } = Algorithm.GeneticAlgorithm;
    private DvrpSolverParameters? AlgorithmParameters { get; set; }
    private DvrpModel Data { get; set; } = new();
    private DvrpSolution? Solution { get; set; }

    private void RunSolver()
    {
        var solver = SolverSelection(SelectedAlgorithm);
        
        if (Data is not null && AlgorithmParameters is not null)
        {
            Solution = solver.Solve(Data, AlgorithmParameters);
        }
    }
}
