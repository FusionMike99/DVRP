namespace DVRP.Domain.Entities;

public class GeneticAlgorithmParameters : DvrpSolverParameters
{
    public int MaxGenerations { get; set; } = 100;
    public int PopulationSize { get; set; } = 50;
    public double MutationRate { get; set; } = 0.01;
    public double CrossoverRate { get; set; } = 0.4;
}
