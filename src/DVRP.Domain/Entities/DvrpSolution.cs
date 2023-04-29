namespace DVRP.Domain.Entities;

public class DvrpSolution
{
    public List<VehicleRoute> Routes { get; set; }
    public double TotalDistance => Routes.Sum(r => r.Distance); 

    public DvrpSolution()
    {
        Routes = new List<VehicleRoute>();
    }
}
