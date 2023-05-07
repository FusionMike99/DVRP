using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace DVRP.UI.Components.Results;

public partial class TableResult
{
    private DvrpSolution? _solution;

    [Parameter]
    public DvrpSolution? Solution
    {
        get => _solution;
        set
        {
            if (_solution != value)
            {
                _solution = value;
                SolutionChanged.InvokeAsync(value);
            }
        }
    }

    [Parameter]
    public EventCallback<DvrpSolution?> SolutionChanged { get; set; }
}
