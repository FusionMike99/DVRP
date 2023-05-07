namespace DVRP.Domain.Entities;

public record GaAcoParameters : DvrpSolverParameters
{
    public GeneticAlgorithmParameters GeneticAlgorithmParameters { get; set; } = new();
    public AntColonyParameters AntColonyParameters { get; set; } = new();
}
