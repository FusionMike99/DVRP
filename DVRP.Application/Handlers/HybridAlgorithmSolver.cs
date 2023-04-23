using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class HybridAlgorithmSolver : IDvrpSolver
{
    public Algorithm Algorithm => Algorithm.HybridAlgorithm;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters)
    {
        if (parameters is not HybridAlgorithmParameters hybridParameters)
        {
            throw new ArgumentException("The provided parameters must be of type HybridAlgorithmParameters.");
        }

        GeneticAlgorithmSolver gaSolver = new();
        AntColonyOptimizationSolver acoSolver = new();

        DvrpSolution gaSolution = gaSolver.Solve(model, hybridParameters.GeneticAlgorithmParameters);
        DvrpSolution acoSolution = acoSolver.Solve(model, hybridParameters.AntColonyParameters, gaSolution);

        return gaSolution.TotalDistance <= acoSolution.TotalDistance ? gaSolution : acoSolution;
    }
}
