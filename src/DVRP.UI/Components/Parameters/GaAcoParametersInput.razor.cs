using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters
{
    public partial class GaAcoParametersInput
    {
        private GaAcoParameters _parameters = new();

        [Parameter]
        public DvrpSolverParameters Parameters
        {
            get => _parameters;
            set
            {
                if (value is GaAcoParameters castedValue && _parameters != castedValue)
                {
                    _parameters = castedValue;
                    ParametersChanged.InvokeAsync(castedValue);
                }
            }
        }

        [Parameter]
        public EventCallback<DvrpSolverParameters> ParametersChanged { get; set; }
    }
}
