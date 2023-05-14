namespace DVRP.Domain.Entities;

public class AntColonyParameters : DvrpSolverParameters
{
    public int MaxIterations { get; set; } = 100;
    public int NumberOfAnts { get; set; } = 10;
    public double Alpha { get; set; } = 1;
    public double Beta { get; set; } = 2;
    public double EvaporationRate { get; set; } = 0.1;
    public double Q { get; set; } = 10;
}
