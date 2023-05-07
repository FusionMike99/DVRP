using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class CustomersInput
{
    [Parameter]
    public List<Customer> Customers { get; set; } = new();

    private Customer? _selectedCustomer;
}
