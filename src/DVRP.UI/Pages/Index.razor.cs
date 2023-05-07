using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Pages;

public partial class Index
{
    [Inject]
    private DvrpSolverSelection SolverSelection { get; set; } = null!;

    private readonly Dictionary<Algorithm, DvrpSolverParameters> parametersDictionary = new()
    {
        { Algorithm.GeneticAlgorithm, new GeneticAlgorithmParameters() },
        { Algorithm.AntColonyOptimization, new AntColonyParameters() },
        { Algorithm.TabuSearch, new TabuSearchParameters() },
        { Algorithm.GaAcoAlgorithm, new GaAcoParameters() },
        { Algorithm.GaTsAlgorithm, new GaTsParameters() },
        { Algorithm.TsAcoAlgorithm, new TsAcoParameters() }
    };

    private Algorithm SelectedAlgorithm { get; set; } = Algorithm.GeneticAlgorithm;
    private DvrpSolverParameters AlgorithmParameters { get; set; } = new GeneticAlgorithmParameters();
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

    private void OnSelectedAlgorithmChanged(Algorithm algorithm)
    {
        SelectedAlgorithm = algorithm;
        parametersDictionary.TryGetValue(algorithm, out var parameters);
        AlgorithmParameters = parameters ?? new GeneticAlgorithmParameters();
    }
}
