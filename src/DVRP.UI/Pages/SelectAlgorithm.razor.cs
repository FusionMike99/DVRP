using DVRP.Domain.Entities;
using DVRP.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Pages
{
    public partial class SelectAlgorithm
    {
        private string SelectedTab => _selectedAlgorithm.ToString();

        private Algorithm _selectedAlgorithm = Algorithm.GeneticAlgorithm;
        private DvrpSolverParameters? _algorithmParameters;

        [Parameter]
        public Algorithm SelectedAlgorithm
        {
            get => _selectedAlgorithm;
            set
            {
                if (_selectedAlgorithm != value)
                {
                    _selectedAlgorithm = value;
                    SelectedAlgorithmChanged.InvokeAsync(value);
                }
            }
        }

        [Parameter]
        public DvrpSolverParameters? AlgorithmParameters
        {
            get => _algorithmParameters;
            set
            {
                if (_algorithmParameters != value)
                {
                    _algorithmParameters = value;
                    AlgorithmParametersChanged.InvokeAsync(value);
                }
            }
        }

        [Parameter]
        public EventCallback<Algorithm> SelectedAlgorithmChanged { get; set; }

        [Parameter]
        public EventCallback<DvrpSolverParameters?> AlgorithmParametersChanged { get; set; }

        private Task OnSelectedTabChanged(string algorithm)
        {
            var isParsed = Enum.TryParse<Algorithm>(algorithm, out var parsedAlgorithm);
            SelectedAlgorithm = isParsed ? parsedAlgorithm : Algorithm.GeneticAlgorithm;

            return Task.CompletedTask;
        }
    }
}
