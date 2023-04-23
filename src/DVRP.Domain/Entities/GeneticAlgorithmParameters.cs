using DVRP.Domain.Enums;

namespace DVRP.Domain.Entities;

public class GeneticAlgorithmParameters : DvrpSolverParameters
{
    public int PopulationSize { get; set; }
    public double MutationRate { get; set; }
    public double CrossoverRate { get; set; }
    public int TournamentSize { get; set; }
    public int MaxGenerations { get; set; }
    public GeneticAlgorithmSelectionMethod SelectionMethod { get; set; }
}
