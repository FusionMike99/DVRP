namespace DVRP.Domain.Entities;

public class GaAcoParameters : DvrpSolverParameters
{
    public GeneticAlgorithmParameters GeneticAlgorithmParameters { get; set; } = new();
    public AntColonyParameters AntColonyParameters { get; set; } = new();
}
