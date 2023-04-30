namespace DVRP.Domain.Entities;

public class DistanceMatrix : BaseMatrix
{
    public DistanceMatrix(DvrpModel model) 
    {
        InitializeData(model);
    }

    private void InitializeData(DvrpModel model)
    {
        int numberOfLocations = model.Depots.Count + model.Customers.Count;
        
        List<Location> locations = new(numberOfLocations);
        locations.AddRange(model.Depots);
        locations.AddRange(model.Customers);
        
        Data = new Dictionary<string, IDictionary<string, double>>(numberOfLocations);

        foreach(var location1 in locations)
        {
            Data[location1.Id] = new Dictionary<string, double>(numberOfLocations);

            foreach (var location2 in locations)
            {
                if (location1.Id == location2.Id)
                {
                    this[location1.Id, location2.Id] = 0;
                }
                else
                {
                    this[location1.Id, location2.Id] = location1.CalculateDistance(location2);
                }
            }
        }
    }
}
