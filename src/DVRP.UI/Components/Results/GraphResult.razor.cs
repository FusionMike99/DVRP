﻿using Blazorise.Charts.Streaming;
using Blazorise.Charts;

namespace DVRP.UI.Components.Results;

public partial class GraphResult
{
    LineChart<LiveDataPoint> horizontalLineChart;
    Random random = new Random(DateTime.Now.Millisecond);

    string[] Labels = { "Red", "Blue", "Yellow", "Green", "Purple", "Orange" };
    List<string> backgroundColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
    List<string> borderColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };

    public class LiveDataPoint
    {
        public double X { get; set; }

        public double Y { get; set; }
    }

    LineChartOptions horizontalLineChartOptions = new()
    {
        Scales = new()
        {
            Y = new()
            {
                Title = new()
                {
                    Display = true,
                    Text = "Value"
                }
            },
            X = new()
            {
                
            }
        }
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.WhenAll(
                HandleRedraw(horizontalLineChart, GetLineChartDataset1));
        }
    }

    async Task HandleRedraw<TDataSet, TItem, TOptions, TModel>(BaseChart<TDataSet, TItem, TOptions, TModel> chart, params Func<TDataSet>[] getDataSets)
        where TDataSet : ChartDataset<TItem>
        where TOptions : ChartOptions
        where TModel : ChartModel
    {
        await chart.Clear();

        await chart.AddLabelsDatasetsAndUpdate(Labels, getDataSets.Select(x => x.Invoke()).ToArray());
    }

    LineChartDataset<LiveDataPoint> GetLineChartDataset1()
    {
        return new LineChartDataset<LiveDataPoint>
        {
            Data = new List<LiveDataPoint>(),
            Label = "Dataset 1 (linear interpolation)",
            BackgroundColor = backgroundColors[0],
            BorderColor = borderColors[0],
            Fill = false,
            Tension = 0,
            BorderDash = new List<int> { 8, 4 },
        };
    }

    Task OnHorizontalLineRefreshed(ChartStreamingData<LiveDataPoint> data)
    {
        data.Value = new LiveDataPoint
        {
            X = RandomScalingFactor(),
            Y = RandomScalingFactor(),
        };

        return Task.CompletedTask;
    }

    double RandomScalingFactor()
    {
        return (random.NextDouble() > 0.5 ? 1.0 : -1.0) * Math.Round(random.NextDouble() * 100);
    }
}
