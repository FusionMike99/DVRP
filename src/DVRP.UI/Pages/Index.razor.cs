using DVRP.Domain.Entities;
using DVRP.Domain.Enums;

namespace DVRP.UI.Pages
{
    public partial class Index
    {
        private Algorithm SelectedAlgorithm { get; set; } = Algorithm.GeneticAlgorithm;

        private DvrpSolverParameters? AlgorithmParameters { get; set; }
    }
}
