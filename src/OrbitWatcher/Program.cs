using OrbitWatcher.HostedServices;
using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.SignalR;
using OrbitWatcher.Services;
using OrbitWatcher.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterSettings(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ClientDevelopment",
        policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                    Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                    && string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder.Services.AddSingleton<SatelliteStorage>();
builder.Services.AddHostedService<OmmDownloaderHostedService>();
builder.Services.AddHostedService<SatelliteStreamerHostedService>();
builder.Services.AddHttpClient<ICelestrackClient, CelestrackClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("ClientDevelopment");
}

app.MapHub<SatellitesHub>(SatellitesHub.Route);

await app.RunAsync();
