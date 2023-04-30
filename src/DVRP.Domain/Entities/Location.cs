namespace DVRP.Domain.Entities;

public abstract record Location
{
    public abstract string Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public double CalculateDistance(Location location)
    {
        double dx = X - location.X;
        double dy = Y - location.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
