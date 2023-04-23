namespace DVRP.Domain.Entities;

public class Depot : Location
{
    public double StartTime { get; set; }
    public double EndTime { get; set; }
}
