using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class GaTsSolver : IDvrpSolver
{
    public Algorithm Algorithm => Algorithm.GaTsAlgorithm;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not GaTsParameters hybridParameters)
        {
            throw new ArgumentException("The provided parameters must be of type HybridAlgorithmParameters.");
        }

        GeneticAlgorithmSolver gaSolver = new();
        TabuSearchSolver tsSolver = new();

        DvrpSolution gaSolution = new();
        DvrpSolution tsSolution = new();

        // Run the algorithms in parallel using the Parallel.Invoke method
        Parallel.Invoke(
            () => gaSolution = gaSolver.Solve(model, hybridParameters.GeneticAlgorithmParameters),
            () => tsSolution = tsSolver.Solve(model, hybridParameters.TabuSearchParameters));

        return gaSolution.Fitness <= tsSolution.Fitness ? gaSolution : tsSolution;
    }
}
