namespace DVRP.Domain.Entities;

public abstract class BaseMatrix
{
    public IDictionary<string, IDictionary<string, double>> Data { get; protected set; } = null!;

    public double this[string i, string j]
    {
        get => Data[i][j];
        set 
        {
            Data[i][j] = value; 
        }
    }

    public int Length => Data.Count;
}
