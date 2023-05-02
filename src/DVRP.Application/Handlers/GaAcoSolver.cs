using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class GaAcoSolver : IDvrpSolver
{
    public Algorithm Algorithm => Algorithm.GaAcoAlgorithm;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not GaAcoParameters hybridParameters)
        {
            throw new ArgumentException("The provided parameters must be of type HybridAlgorithmParameters.");
        }

        GeneticAlgorithmSolver gaSolver = new();
        AntColonyOptimizationSolver acoSolver = new();

        DvrpSolution gaSolution = new();
        DvrpSolution acoSolution = new();

        // Run the algorithms in parallel using the Parallel.Invoke method
        Parallel.Invoke(
            () => gaSolution = gaSolver.Solve(model, hybridParameters.GeneticAlgorithmParameters),
            () => acoSolution = acoSolver.Solve(model, hybridParameters.AntColonyParameters));

        return gaSolution.Fitness <= acoSolution.Fitness ? gaSolution : acoSolution;
    }
}
