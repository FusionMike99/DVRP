namespace DVRP.Domain.Entities;

public record GaTsParameters : DvrpSolverParameters
{
    public GeneticAlgorithmParameters GeneticAlgorithmParameters { get; set; } = new();
    public TabuSearchParameters TabuSearchParameters { get; set; } = new();
}
