﻿using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Parameters
{
    public partial class GaParametersInput
    {
        private GeneticAlgorithmParameters _parameters = new();

        [Parameter]
        public DvrpSolverParameters Parameters
        {
            get => _parameters;
            set
            {
                if (value is GeneticAlgorithmParameters castedValue && _parameters != castedValue)
                {
                    _parameters = castedValue;
                    ParametersChanged.InvokeAsync(castedValue);
                }
            }
        }

        [Parameter]
        public EventCallback<DvrpSolverParameters> ParametersChanged { get; set; }
    }
}
