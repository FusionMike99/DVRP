namespace DVRP.Domain.Entities;

public class HybridAlgorithmParameters : DvrpSolverParameters
{
    public GeneticAlgorithmParameters GeneticAlgorithmParameters { get; set; } = new();
    public AntColonyParameters AntColonyParameters { get; set; } = new();
    public double HybridRatio { get; set; }
}
