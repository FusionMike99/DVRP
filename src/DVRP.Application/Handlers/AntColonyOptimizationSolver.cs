using DVRP.Application.Abstractions;
using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.Application.Handlers;

public class AntColonyOptimizationSolver : IDvrpSolver
{
    private DvrpModel _model = null!;
    private DistanceMatrix _distanceMatrix = null!;
    private PheromoneMatrix _pheromoneMatrix = null!;

    public Algorithm Algorithm => Algorithm.AntColonyOptimization;

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters)
    {
        return Solve(model, parameters, null);
    }

    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if (parameters is not AntColonyParameters antColonyParameters)
        {
            throw new ArgumentException("The provided parameters must be of type AntColonyParameters.");
        }

        Initialize(model);

        int numberOfAnts = antColonyParameters.NumberOfAnts;
        int numberOfIterations = antColonyParameters.MaxIterations;
        double alpha = antColonyParameters.Alpha;
        double beta = antColonyParameters.Beta;
        double rho = antColonyParameters.EvaporationRate;
        double q = antColonyParameters.Q;

        DvrpSolution? bestSolution = initialSolution;

        int currentVehicle = 0;

        for (int iteration = 0; iteration < numberOfIterations; iteration++)
        {
            List<DvrpSolution> antSolutions = new(numberOfAnts);

            for (int ant = 0; ant < numberOfAnts; ant++)
            {
                Ant antInstance = new(_model, _distanceMatrix, _pheromoneMatrix);
                DvrpSolution antSolution = antInstance.BuildSolution(alpha, beta, currentVehicle);
                antSolutions.Add(antSolution);

                if (bestSolution is null || antSolution.TotalDistance < bestSolution.TotalDistance)
                {
                    bestSolution = antSolution;
                }
            }

            _pheromoneMatrix.UpdatePheromoneTrails(antSolutions, rho, q);

            currentVehicle = (currentVehicle + 1) % _model.Vehicles.Count;
        }

        return bestSolution!;
    }

    private void Initialize(DvrpModel model)
    {
        _model = model;
        _distanceMatrix = new DistanceMatrix(model);
        _pheromoneMatrix = new PheromoneMatrix(model);
    }
}
