namespace DVRP.Domain.Entities;

public class DvrpModel
{
    public List<Customer> Customers { get; set; } = new();
    public List<Depot> Depots { get; set; } = new();
    public List<Vehicle> Vehicles { get; set; } = new();
}
