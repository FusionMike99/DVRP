namespace DVRP.Domain.Entities;

public class DvrpSolution
{
    public List<VehicleRoute> Routes { get; set; } = new();
    public double TotalDistance => Routes.Sum(r => r.Distance);
    public double Fitness { get; private set; }

    public void CalculateFitness(int depotsCount)
    {
        double totalDistance = TotalDistance;

        // Apply a penalty for violating capacity constraints
        double capacityPenalty = 0;
        foreach (VehicleRoute route in Routes)
        {
            double routeDemand = route.Locations.OfType<Customer>().Sum(c => c.Demand);

            if (routeDemand > route.Vehicle.Capacity)
            {
                capacityPenalty += (routeDemand - route.Vehicle.Capacity) * depotsCount; // Apply a suitable penalty factor
            }
        }

        // The fitness value should be minimized, so a higher penalty value will result in a worse fitness
        Fitness = totalDistance + capacityPenalty;
    }
}
