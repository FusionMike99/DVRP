using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters
{
    public partial class HybridParametersInput
    {
        private GaAcoParameters _parameters = new();

        [Parameter]
        public GaAcoParameters Parameters
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
        public EventCallback<GaAcoParameters> ParametersChanged { get; set; }
    }
}
