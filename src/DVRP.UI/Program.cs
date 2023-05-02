using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using DVRP.Application.Abstractions;
using DVRP.Application.Handlers;
using DVRP.Domain.Enums;
using DVRP.UI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazorise(options => options.Immediate = true)
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

builder.Services.AddScoped<IDvrpSolver, GeneticAlgorithmSolver>();
builder.Services.AddScoped<IDvrpSolver, AntColonyOptimizationSolver>();
builder.Services.AddScoped<IDvrpSolver, GaAcoSolver>();
builder.Services.AddScoped<DvrpSolverSelection>(sp => key =>
{
    return key switch
    {
        Algorithm.GeneticAlgorithm => sp.GetRequiredService<GeneticAlgorithmSolver>(),
        Algorithm.AntColonyOptimization => sp.GetRequiredService<AntColonyOptimizationSolver>(),
        Algorithm.GaAcoAlgorithm => sp.GetRequiredService<GaAcoSolver>(),
        _ => throw new KeyNotFoundException()
    };
});

await builder.Build().RunAsync();
