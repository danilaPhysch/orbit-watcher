using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrbitWatcher.Client;
using OrbitWatcher.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<SatelliteSignalRSettings>(settings =>
{
    builder.Configuration.GetSection(SatelliteSignalRSettings.SectionName).Bind(settings);
    
    var apiAddress = builder.Configuration["services:api:https:0"] ?? builder.Configuration["services:api:http:0"];
    if (!string.IsNullOrEmpty(apiAddress))
    {
        settings.HubBaseUrl = apiAddress;
    }
});
builder.Services.AddScoped<SatellitePositionsStream>();
builder.Services.AddScoped<SatelliteStateService>();
builder.Services.AddScoped<LeafletMapInterop>();

await builder.Build().RunAsync();
