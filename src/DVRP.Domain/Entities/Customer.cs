namespace DVRP.Domain.Entities;

public record Customer : Location
{
    public double Demand { get; set; }
}
