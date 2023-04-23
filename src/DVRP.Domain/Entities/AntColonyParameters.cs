namespace DVRP.Domain.Entities;

public class AntColonyParameters : DvrpSolverParameters
{
    public int NumberOfAnts { get; set; }
    public double Alpha { get; set; }
    public double Beta { get; set; }
    public double EvaporationRate { get; set; }
    public int MaxIterations { get; set; }
    public double Q { get; set; }
}
