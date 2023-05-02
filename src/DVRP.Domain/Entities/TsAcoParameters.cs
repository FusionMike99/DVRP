namespace DVRP.Domain.Entities;

public class TsAcoParameters : DvrpSolverParameters
{
    public TabuSearchParameters TabuSearchParameters { get; set; } = new();
    public AntColonyParameters AntColonyParameters { get; set; } = new();
}
