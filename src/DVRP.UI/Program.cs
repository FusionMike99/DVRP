using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using DVRP.Application.Abstractions;
using DVRP.Application.Handlers;
using DVRP.Domain.Enums;
using DVRP.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazorise(options => options.Immediate = true)
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

builder.Services.AddScoped<GeneticAlgorithmSolver>();
builder.Services.AddScoped<AntColonyOptimizationSolver>();
builder.Services.AddScoped<TabuSearchSolver>();
builder.Services.AddScoped<GaAcoSolver>();
builder.Services.AddScoped<GaTsSolver>();
builder.Services.AddScoped<TsAcoSolver>();
builder.Services.AddScoped<DvrpSolverSelection>(sp => key =>
{
    return key switch
    {
        Algorithm.GeneticAlgorithm => sp.GetRequiredService<GeneticAlgorithmSolver>(),
        Algorithm.AntColonyOptimization => sp.GetRequiredService<AntColonyOptimizationSolver>(),
        Algorithm.TabuSearch => sp.GetRequiredService<TabuSearchSolver>(),
        Algorithm.GaAcoAlgorithm => sp.GetRequiredService<GaAcoSolver>(),
        Algorithm.GaTsAlgorithm => sp.GetRequiredService<GaTsSolver>(),
        Algorithm.TsAcoAlgorithm => sp.GetRequiredService<TsAcoSolver>(),
        _ => throw new KeyNotFoundException()
    };
});

await builder.Build().RunAsync();
