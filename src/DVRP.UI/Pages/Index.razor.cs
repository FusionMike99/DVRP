using DVRP.Domain.Enums;
using DVRP.Domain.Entities;

namespace DVRP.UI.Pages
{
    public partial class Index
    {
        private Algorithm SelectedAlgorithm { get; set; } = Algorithm.GeneticAlgorithm;


        private Customer customer = new();
    }
}
