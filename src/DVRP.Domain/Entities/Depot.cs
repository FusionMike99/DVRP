namespace DVRP.Domain.Entities;

public record Depot : Location
{
    private static int Counter = 1;

    public override string Id { get; set; } = $"D{Counter++}";
}
