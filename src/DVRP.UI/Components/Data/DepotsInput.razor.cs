using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class DepotsInput
{
    private Depot? _selectedDepot;

    private List<Depot> _depots = new();

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
}
