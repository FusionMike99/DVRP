using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class VehiclesInput
{
    [Parameter]
    public List<Vehicle> Vehicles { get; set; }

    private Vehicle _selectedVehicle;
}
