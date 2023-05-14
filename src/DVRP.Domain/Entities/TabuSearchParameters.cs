namespace DVRP.Domain.Entities;

public class TabuSearchParameters : DvrpSolverParameters
{
    public int TabuListSize { get; set; } = 10;
    public int MaxIterations { get; set; } = 100;
    public int NeighborhoodSearchSize { get; set; } = 10;
    public double IntensificationFactor { get; set; } = 0.5;
    public double DiversificationFactor { get; set; } = 0.5;
}
