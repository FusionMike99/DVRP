namespace DVRP.Domain.Entities;

public class VehicleRoute
{
    public int VehicleId { get; set; }
    public List<int> LocationIds { get; set; } = new();
    public double Distance { get; set; }
}
