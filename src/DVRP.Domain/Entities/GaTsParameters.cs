namespace DVRP.Domain.Entities;

public class GaTsParameters : DvrpSolverParameters
{
    public GeneticAlgorithmParameters GeneticAlgorithmParameters { get; set; } = new();
    public TabuSearchParameters TabuSearchParameters { get; set; } = new();
}
