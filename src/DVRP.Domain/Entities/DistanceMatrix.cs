namespace DVRP.Domain.Entities;

public class DistanceMatrix
{
    private readonly double[][] _distances;

    public DistanceMatrix(DvrpModel model)
    {
        int numberOfDepots = model.Depots.Count;
        int numberOfCustomers = model.Customers.Count;
        int numberOfLocations = numberOfDepots + numberOfCustomers;

        _distances = new double[numberOfLocations][];

        for (int i = 0; i < numberOfLocations; i++)
        {
            _distances[i] = new double[numberOfLocations];
            for (int j = 0; j < numberOfLocations; j++)
            {
                Location location1 = i < numberOfDepots ? model.Depots[i] : model.Customers[i - numberOfDepots];
                Location location2 = j < numberOfDepots ? model.Depots[j] : model.Customers[j - numberOfDepots];
                _distances[i][j] = CalculateDistance(location1, location2);
            }
        }
    }

    public double GetDistance(int location1, int location2)
    {
        return _distances[location1][location2];
    }

    private static double CalculateDistance(Location location1, Location location2)
    {
        double deltaX = location1.X - location2.X;
        double deltaY = location1.Y - location2.Y;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}
