using OrbitWatcher.HostedServices;
using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.Services;
using OrbitWatcher.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterSettings(builder.Configuration);

builder.Services.AddSingleton<SatelliteStorage>();
builder.Services.AddHostedService<OmmDownloaderHostedService>();
builder.Services.AddHttpClient<ICelestrackClient, CelestrackClient>();

var app = builder.Build();

await app.RunAsync();