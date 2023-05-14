using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class DvrpDataInput
{
    private string _selectedTab = "depots";
    private DvrpModel _data = new();

    [Parameter]
    public DvrpModel Data
    {
        get => _data;
        set
        {
            if(_data != value)
            {
                _data = value;
                DataChanged.InvokeAsync(value);
            }
        }
    }

    [Parameter]
    public EventCallback<DvrpModel> DataChanged { get; set; }

    private Task OnSelectedTabChanged(string name)
    {
        _selectedTab = name;

        return Task.CompletedTask;
    }

    private void OnDataChanged()
    {
        DataChanged.InvokeAsync(Data);
    }

    private void UploadTestScenario1()
    {
        Data.Depots = new List<Depot>()
        {
            new Depot { Id = "D1", X = 560, Y = 210 },
            new Depot { Id = "D2", X = 570, Y = 520 },
            new Depot { Id = "D3", X = 910, Y = 550 },
            new Depot { Id = "D4", X = 960, Y = 230 },
        };

        Data.Customers = new List<Customer>
        {
            new Customer { Id = "C1", X = 530, Y = 330, Demand = 20 },
            new Customer { Id = "C2", X = 680, Y = 270, Demand = 20 },
            new Customer { Id = "C3", X = 680, Y = 140, Demand = 20 },
            new Customer { Id = "C4", X = 540, Y = 90, Demand = 20 },
            new Customer { Id = "C5", X = 420, Y = 210, Demand = 20 },
            new Customer { Id = "C6", X = 470, Y = 550, Demand = 20 },
            new Customer { Id = "C7", X = 480, Y = 440, Demand = 20 },
            new Customer { Id = "C8", X = 640, Y = 430, Demand = 20 },
            new Customer { Id = "C9", X = 690, Y = 550, Demand = 20 },
            new Customer { Id = "C10", X = 570, Y = 640, Demand = 20 },
            new Customer { Id = "C11", X = 790, Y = 540, Demand = 20 },
            new Customer { Id = "C12", X = 850, Y = 640, Demand = 20 },
            new Customer { Id = "C13", X = 990, Y = 610, Demand = 20 },
            new Customer { Id = "C14", X = 1010, Y = 490, Demand = 20 },
            new Customer { Id = "C15", X = 860, Y = 440, Demand = 20 },
            new Customer { Id = "C16", X = 830, Y = 230, Demand = 20 },
            new Customer { Id = "C17", X = 910, Y = 130, Demand = 20 },
            new Customer { Id = "C18", X = 1070, Y = 180, Demand = 20 },
            new Customer { Id = "C19", X = 1060, Y = 280, Demand = 20 },
            new Customer { Id = "C20", X = 900, Y = 330, Demand = 20 },
        };

        Data.Vehicles = new List<Vehicle>
        {
            new Vehicle { Id = "V1", Capacity = 60, DepotId = "D1" },
            new Vehicle { Id = "V2", Capacity = 40, DepotId = "D1" },
            new Vehicle { Id = "V3", Capacity = 40, DepotId = "D2" },
            new Vehicle { Id = "V4", Capacity = 40, DepotId = "D2" },
            new Vehicle { Id = "V5", Capacity = 40, DepotId = "D3" },
            new Vehicle { Id = "V6", Capacity = 40, DepotId = "D3" },
            new Vehicle { Id = "V7", Capacity = 40, DepotId = "D4" },
            new Vehicle { Id = "V8", Capacity = 60, DepotId = "D4" },
        };

        OnDataChanged();
    }
}
