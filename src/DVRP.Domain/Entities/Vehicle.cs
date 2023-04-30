namespace DVRP.Domain.Entities;

public record Vehicle : Location
{
    private static int Counter = 1;

    public override string Id { get; set; } = $"V{Counter++}";
    public double Capacity { get; set; }
    public required string DepotId { get; set; }
}
