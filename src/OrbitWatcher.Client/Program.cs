using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrbitWatcher.Client;
using OrbitWatcher.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<SatelliteSignalRSettings>(
    builder.Configuration.GetSection(SatelliteSignalRSettings.SectionName));
builder.Services.AddScoped<SatellitePositionsStream>();

await builder.Build().RunAsync();
