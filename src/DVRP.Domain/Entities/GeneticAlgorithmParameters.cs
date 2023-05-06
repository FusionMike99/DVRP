using DVRP.Domain.Enums;

namespace DVRP.Domain.Entities;

public class GeneticAlgorithmParameters : DvrpSolverParameters
{
    public int MaxGenerations { get; set; }
    public int PopulationSize { get; set; }
    public double MutationRate { get; set; }
    public double CrossoverRate { get; set; }
    public SelectionMethod SelectionMethod { get; set; }
    public int TournamentSize { get; set; }
}
