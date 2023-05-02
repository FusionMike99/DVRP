namespace DVRP.Domain.Entities;

public class TabuSearchParameters : DvrpSolverParameters
{
    public int TabuListSize { get; set; }
    public int MaxIterations { get; set; }
    public int NeighborhoodSearchSize { get; set; }
    public double IntensificationFactor { get; set; }
    public double DiversificationFactor { get; set; }
}
