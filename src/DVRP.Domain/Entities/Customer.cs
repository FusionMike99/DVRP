namespace DVRP.Domain.Entities;

public record Customer : Location
{
    private static int Counter = 1;

    public override string Id { get; set; } = $"C{Counter++}";
    public double Demand { get; set; }
}
