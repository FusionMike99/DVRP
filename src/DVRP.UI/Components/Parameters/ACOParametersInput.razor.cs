using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters
{
    public partial class ACOParametersInput
    {
        private AntColonyParameters _parameters = new();

        [Parameter]
        public AntColonyParameters Parameters
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
        public EventCallback<AntColonyParameters> ParametersChanged { get; set; }
    }
}
