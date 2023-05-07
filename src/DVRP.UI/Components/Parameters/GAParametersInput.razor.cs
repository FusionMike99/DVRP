using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters;

public partial class GaParametersInput
{
    private GeneticAlgorithmParameters _parameters = new();

    private GeneticAlgorithmParameters TypedParameters
    {
        get => _parameters;
        set
        {
            if(_parameters != value)
            {
                _parameters = value;
                ParametersChanged.InvokeAsync(value);
            }
        }
    }

    [Parameter]
    public DvrpSolverParameters Parameters
    {
        get => TypedParameters;
        set
        {
            if (value is GeneticAlgorithmParameters castedValue)
            {
                TypedParameters = castedValue;
            }
        }
    }

    [Parameter]
    public EventCallback<DvrpSolverParameters> ParametersChanged { get; set; }
}
