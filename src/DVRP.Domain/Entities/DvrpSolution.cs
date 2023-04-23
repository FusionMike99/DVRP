namespace DVRP.Domain.Entities;

public class DvrpSolution
{
    public List<VehicleRoute> Routes { get; set; }
    public double TotalDistance { get; set; }
    public double TotalTime { get; set; }

    public DvrpSolution()
    {
        Routes = new List<VehicleRoute>();
        TotalDistance = 0;
    }

    public DvrpSolution(List<VehicleRoute> routes, double totalDistance)
    {
        Routes = routes;
        TotalDistance = totalDistance;
    }
}
