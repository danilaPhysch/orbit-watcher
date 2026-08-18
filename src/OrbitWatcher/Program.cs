using OrbitWatcher.Contracts;
using OrbitWatcher.HostedServices;
using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.SignalR;
using OrbitWatcher.Services;
using OrbitWatcher.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

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

builder.Services.AddSingleton<ISatelliteStorage, SatelliteStorage>();
builder.Services.AddSingleton<GroundTrackService>();
builder.Services.AddHostedService<OmmDownloaderHostedService>();
builder.Services.AddHostedService<SatelliteStreamerHostedService>();
builder.Services.AddHttpClient<ICelestrackClient, CelestrackClient>();

var app = builder.Build();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseCors("ClientDevelopment");
}

app.MapHub<SatellitesHub>(HubConstants.Route);

await app.RunAsync();
