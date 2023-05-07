﻿using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class DepotsInput
{
    [Parameter]
    public List<Depot> Depots { get; set; } = new();

    private Depot? _selectedDepot;
}
