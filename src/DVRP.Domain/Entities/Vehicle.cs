namespace DVRP.Domain.Entities;

public record Vehicle : Location
{
    public double Capacity { get; set; }
    public int DepotId { get; set; }
}
