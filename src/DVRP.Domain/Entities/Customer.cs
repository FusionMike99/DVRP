namespace DVRP.Domain.Entities;

public class Customer : Location
{
    public double Demand { get; set; }
    public double ServiceTime { get; set; }
    public double TimeWindowStart { get; set; }
    public double TimeWindowEnd { get; set; }
}
