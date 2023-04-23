﻿using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Data;

public partial class DVRPDataInput
{
    private string _selectedTab = "customers";
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
}
