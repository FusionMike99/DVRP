using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Abstractions;

public delegate IDvrpSolver DvrpSolverSelection(Algorithm algorithm);

public interface IDvrpSolver
{
    Algorithm Algorithm { get; }
    DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null);
}
