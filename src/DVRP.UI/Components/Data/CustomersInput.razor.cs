using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class CustomersInput
{
    private Customer? _selectedCustomer;

    private List<Customer> _customers = new();

    [Parameter]
    public List<Customer> Customers
    {
        get => _customers;
        set
        {
            if (_customers != value)
            {
                _customers = value;
                CustomersChanged.InvokeAsync(value);
            }
        }
    }

    [Parameter]
    public EventCallback<List<Customer>> CustomersChanged { get; set; }

    private void OnCustomersChanged()
    {
        CustomersChanged.InvokeAsync(Customers);
    }
}
