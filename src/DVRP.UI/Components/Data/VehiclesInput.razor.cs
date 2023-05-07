using Blazorise;
using Blazorise.DataGrid;
using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class VehiclesInput
{   
    private Vehicle? _selectedVehicle;
    
    private List<Depot> _depots = new();
    
    [Parameter]
    public List<Vehicle> Vehicles { get; set; } = new();

    [Parameter]
    public List<Depot> Depots
    {
        get => _depots;
        set
        {
            if (_depots != value)
            {
                _depots = value;
                DepotsChanged.InvokeAsync(value);
            }
        }
    }

    [Parameter]
    public EventCallback<List<Depot>> DepotsChanged { get; set; }

    private static void CheckDepotId(ValidatorEventArgs validationArgs)
    {
        ValidationRule.IsNotEmpty(validationArgs);

        if (validationArgs.Status == ValidationStatus.Error)
        {
            validationArgs.ErrorText = "Depot Id can't be empty";
        }
    }
}
