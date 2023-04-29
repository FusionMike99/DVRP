namespace DVRP.Domain.Entities;

public record Vehicle
{
    public int Id { get; set; }
    public double Capacity { get; set; }
    public int DepotId { get; set; }
}
