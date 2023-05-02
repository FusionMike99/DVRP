using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class TsAcoSolver : IDvrpSolver
{
    public Algorithm Algorithm => Algorithm.TsAcoAlgorithm;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not TsAcoParameters hybridParameters)
        {
            throw new ArgumentException("The provided parameters must be of type HybridAlgorithmParameters.");
        }

        TabuSearchSolver tsSolver = new();
        AntColonyOptimizationSolver acoSolver = new();
        
        DvrpSolution tabuSolution = new();
        DvrpSolution antColonySolution = new();

        // Run the algorithms in parallel using the Parallel.Invoke method
        Parallel.Invoke(
            () => tabuSolution = tsSolver.Solve(model, hybridParameters.TabuSearchParameters),
            () => antColonySolution = acoSolver.Solve(model, hybridParameters.AntColonyParameters));

        return tabuSolution.Fitness < antColonySolution.Fitness ? tabuSolution : antColonySolution;
    }
}