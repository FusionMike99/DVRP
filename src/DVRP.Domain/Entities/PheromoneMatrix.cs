namespace DVRP.Domain.Entities;

public class PheromoneMatrix
{
    private readonly double[][] _pheromoneTrails;

    public PheromoneMatrix(DvrpModel model)
    {
        int numberOfDepots = model.Depots.Count;
        int numberOfCustomers = model.Customers.Count;
        int numberOfLocations = numberOfDepots + numberOfCustomers;

        _pheromoneTrails = new double[numberOfLocations][];

        for (int i = 0; i < numberOfLocations; i++)
        {
            _pheromoneTrails[i] = new double[numberOfLocations];
            for (int j = 0; j < numberOfLocations; j++)
            {
                _pheromoneTrails[i][j] = 1.0;
            }
        }
    }

    public double GetPheromoneTrail(int location1, int location2)
    {
        return _pheromoneTrails[location1][location2];
    }

    public void UpdatePheromoneTrails(List<DvrpSolution> antSolutions, double rho, double q)
    {
        // Evaporate pheromone trails
        for (int i = 0; i < _pheromoneTrails.Length; i++)
        {
            for (int j = 0; j < _pheromoneTrails[i].Length; j++)
            {
                _pheromoneTrails[i][j] *= (1 - rho);
            }
        }

        // Deposit pheromones based on the solutions
        foreach (DvrpSolution antSolution in antSolutions)
        {
            double pheromoneDeposit = q / antSolution.TotalDistance;

            foreach (VehicleRoute vehicleRoute in antSolution.Routes)
            {
                List<int> locationIds = vehicleRoute.Locations.Select(l => l.Id).ToList();
                for (int i = 0; i < locationIds.Count - 1; i++)
                {
                    int from = locationIds[i];
                    int to = locationIds[i + 1];
                    _pheromoneTrails[from][to] += pheromoneDeposit;
                    _pheromoneTrails[to][from] += pheromoneDeposit; // Assuming symmetric pheromone trails
                }
            }
        }
    }
}
