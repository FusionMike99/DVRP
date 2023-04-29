namespace DVRP.Domain.Entities;

public record Location
{
    public int Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}
