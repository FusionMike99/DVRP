namespace DVRP.Domain.Entities;

public record GeneticAlgorithmParameters : DvrpSolverParameters
{
    public int MaxGenerations { get; set; }
    public int PopulationSize { get; set; }
    public double MutationRate { get; set; }
    public double CrossoverRate { get; set; }
}
