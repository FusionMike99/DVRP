using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters
{
    public partial class GAParametersInput
    {
        private GeneticAlgorithmParameters _parameters = new();

        [Parameter]
        public GeneticAlgorithmParameters Parameters
        {
            get => _parameters;
            set
            {
                if (_parameters != value)
                {
                    _parameters = value;
                    ParametersChanged.InvokeAsync(value);
                }
            }
        }

        [Parameter]
        public EventCallback<GeneticAlgorithmParameters> ParametersChanged { get; set; }
    }
}
