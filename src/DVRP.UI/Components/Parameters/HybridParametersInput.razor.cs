using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters
{
    public partial class HybridParametersInput
    {
        private HybridAlgorithmParameters _parameters = new();

        [Parameter]
        public HybridAlgorithmParameters Parameters
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
        public EventCallback<HybridAlgorithmParameters> ParametersChanged { get; set; }
    }
}
