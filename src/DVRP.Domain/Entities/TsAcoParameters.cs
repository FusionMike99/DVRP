namespace DVRP.Domain.Entities;

public record TsAcoParameters : DvrpSolverParameters
{
    public TabuSearchParameters TabuSearchParameters { get; set; } = new();
    public AntColonyParameters AntColonyParameters { get; set; } = new();
}
