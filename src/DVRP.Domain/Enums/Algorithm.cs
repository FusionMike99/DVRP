namespace DVRP.Domain.Enums;

[Flags]
public enum Algorithm : byte
{
    GeneticAlgorithm = 1,
    AntColonyOptimization = 2,
    HybridAlgorithm = GeneticAlgorithm | AntColonyOptimization
}
