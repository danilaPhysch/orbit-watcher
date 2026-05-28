using OrbitWatcher.HostedServices;
using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.SignalR;
using OrbitWatcher.Services;
using OrbitWatcher.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterSettings(builder.Configuration);
builder.Services.AddSignalR();

builder.Services.AddSingleton<SatelliteStorage>();
builder.Services.AddHostedService<OmmDownloaderHostedService>();
builder.Services.AddHostedService<SatelliteStreamerHostedService>();
builder.Services.AddHttpClient<ICelestrackClient, CelestrackClient>();

var app = builder.Build();
app.MapHub<SatellitesHub>(SatellitesHub.Route);

await app.RunAsync();
