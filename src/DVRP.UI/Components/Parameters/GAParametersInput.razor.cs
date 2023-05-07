using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters;

public partial class GaParametersInput
{
    private GeneticAlgorithmParameters _parameters = new();

    [Parameter]
    public DvrpSolverParameters Parameters
    {
        get => _parameters;
        set
        {
            if (value is GeneticAlgorithmParameters typedValue && _parameters != typedValue)
            {
                _parameters = typedValue;
                ParametersChanged.InvokeAsync(typedValue);
            }
        }
    }

    [Parameter]
    public EventCallback<DvrpSolverParameters> ParametersChanged { get; set; }
}
