using MoreLinq.Extensions;

namespace DVRP.Domain.Entities;

public class VehicleRoute
{
    public required Vehicle Vehicle { get; set; }
    public List<Location> Locations { get; set; } = new();
    public double Distance => Locations.Pairwise((l1, l2) => l1.CalculateDistance(l2)).Sum();
}
