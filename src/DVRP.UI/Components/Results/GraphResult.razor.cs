using Blazorise.Charts;
using Blazorise.Charts.DataLabels;
using DVRP.Domain.Entities;
using Microsoft.AspNetCore.Components;
using System.Drawing;

namespace DVRP.UI.Components.Results;

public partial class GraphResult
{
    private LineChart<int> lineChart;

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

    // define regular chart options
    private readonly LineChartOptions lineChartOptions = new()
    {
        AspectRatio = 5d / 3d,
        Layout = new()
        {
            Padding = new()
            {
                Top = 32,
                Right = 16,
                Bottom = 16,
                Left = 8
            }
        },
        Elements = new()
        {
            Line = new()
            {
                Fill = false,
                Tension = 0.4,
            }
        },
        Scales = new()
        {
            Y = new()
            {
                Stacked = false,
            }
        },
        Plugins = new()
        {
            Legend = new()
            {
                Display = true
            }
        }
    };

    // define specific dataset styles by targeting them with the DatasetIndex
    private readonly List<ChartDataLabelsDataset> lineDataLabelsDatasets = new()
    {
        new()
        {
            DatasetIndex = 0,
            Options = new()
            {
                BackgroundColor = BackgroundColors[0],
                BorderColor = BorderColors[1]
            }
        },
        new()
        {
            DatasetIndex = 1,
            Options = new ()
            {
                BackgroundColor = BackgroundColors[1],
                BorderColor = BorderColors[2],
            }
        },
        new()
        {
            DatasetIndex = 2,
            Options = new ()
            {
                BackgroundColor = BackgroundColors[2],
                BorderColor = BorderColors[0]
            }
        },
    };

    // some shared options for all data-labels
    private readonly ChartDataLabelsOptions lineDataLabelsOptions = new()
    {
        BorderRadius = 4,
        Color = "#ffffff",
        Font = new()
        {
            Weight = "bold"
        },
        Formatter = ChartMathFormatter.Round,
        Padding = new(6)
    };

    private static string[] Labels = new string[] { "1", "2", "3", "4", "5", "6" };
    private static string[] BackgroundColors = new string[] { "#4bc0c0", "#36a2eb", "#ff3d88" };
    private static string[] BorderColors = new string[] { "#4bc0c0", "#36a2eb", "#ff3d88" };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await HandleRedraw(lineChart, GetLineChartDataset);

            await lineChart.Clear();

            await lineChart.AddLabelsDatasetsAndUpdate(Labels,
                GetLineChartDataset(0),
                GetLineChartDataset(1),
                GetLineChartDataset(2));
        }
    }

    private async Task HandleRedraw<TDataSet, TItem, TOptions, TModel>(BaseChart<TDataSet, TItem, TOptions, TModel> chart, Func<int, TDataSet> getDataSet)
        where TDataSet : ChartDataset<TItem>
        where TOptions : ChartOptions
        where TModel : ChartModel
    {
        await chart.Clear();

        await chart.AddLabelsDatasetsAndUpdate(Labels,
            getDataSet(0),
            getDataSet(1),
            getDataSet(2));
    }

    private LineChartDataset<int> GetLineChartDataset(int colorIndex)
    {
        return new()
        {
            Label = "# of randoms",
            Data = RandomizeData(7, 9),
            BackgroundColor = BackgroundColors[2-colorIndex],
            BorderColor = BorderColors[2-colorIndex],
        };
    }

    private static List<int> RandomizeData(int min, int max)
    {
        return Enumerable.Range(0, Labels.Count()).Select(x => Random.Shared.Next(min, max)).ToList();
    }

    private static IDictionary<string, string> RandomColors(params string[] labels)
    { 
        return labels.ToDictionary(l => l, _ => GetRandomHexColor());
    }

    private static string GetRandomHexColor()
    {
        var color = Color.FromArgb(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256));
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
