namespace DVRP.Domain.Entities;

public class PheromoneMatrix : BaseMatrix
{
    public PheromoneMatrix(DvrpModel model)
    {
        InitializeData(model);
    }

    private void InitializeData(DvrpModel model)
    {
        int numberOfLocations = model.Depots.Count + model.Customers.Count;
        double initialPheromoneValue = 0.01;

        List<Location> locations = new(numberOfLocations);
        locations.AddRange(model.Depots);
        locations.AddRange(model.Customers);

        Data = new Dictionary<string, IDictionary<string, double>>(numberOfLocations);

        foreach (var locationId1 in locations.Select(l => l.Id))
        {
            Data[locationId1] = new Dictionary<string, double>(numberOfLocations);

            foreach (var locationId2 in locations.Select(l => l.Id))
            {
                if (locationId1 == locationId2)
                {
                    this[locationId1, locationId2] = 0;
                }
                else
                {
                    this[locationId1, locationId2] = initialPheromoneValue;
                }
            }
        }
    }

    public void UpdateData(List<DvrpSolution> antSolutions, AntColonyParameters parameters)
    {
        // Evaporate pheromones
        foreach (var key1 in Data.Keys)
        {
            foreach (var key2 in Data[key1].Keys)
            {
                this[key1, key2] *= 1 - parameters.EvaporationRate;
            }
        }

        // Deposit pheromones based on ant solutions
        foreach (DvrpSolution solution in antSolutions)
        {
            double deltaPheromone = parameters.Q / solution.Fitness;

            foreach (var locations in solution.Routes.Select(r => r.Locations))
            {
                for (int i = 0; i < locations.Count - 1; i++)
                {
                    var currentLocationId = locations[i].Id;
                    var nextLocationId = locations[i + 1].Id;

                    this[currentLocationId, nextLocationId] += deltaPheromone;
                    this[nextLocationId, currentLocationId] += deltaPheromone;
                }
            }
        }
    }
}
