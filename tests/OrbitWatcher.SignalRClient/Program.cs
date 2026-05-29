using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrbitWatcher.Contracts;

var builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory
    }
);
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});
builder.Services.AddHostedService<SatelliteSignalRClientHostedService>();

await builder.Build().RunAsync();

internal sealed class SatelliteSignalRClientHostedService(
    IConfiguration configuration,
    IHostApplicationLifetime applicationLifetime,
    ILogger<SatelliteSignalRClientHostedService> logger
) : BackgroundService
{
    private readonly SatelliteSignalRClientSettings _settings = GetSettings(configuration);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var loggedBatches = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var connection = new HubConnectionBuilder().WithUrl(_settings.HubUrl).WithAutomaticReconnect().Build();

        connection.On<IReadOnlyCollection<SatellitePositionDto>>(_settings.EventName, positions =>
        {
            loggedBatches++;
            logger.LogInformation("Batch #{BatchNumber}: received {Count} satellites.", loggedBatches, positions.Count);

            foreach (var satellite in positions)
            {
                logger.LogInformation(
                    "  NORAD={NoradCatId} Name=\"{Name}\" Lat={Lat:F6} Lon={Lon:F6} AltKm={AltKm:F2} Timestamp={TimestampUtc:O}",
                    satellite.NoradCatId,
                    satellite.Name,
                    satellite.Lat,
                    satellite.Lon,
                    satellite.AltKm,
                    satellite.TimestampUtc
                );
            }

            if (loggedBatches >= _settings.MinBatchesToLog)
            {
                done.TrySetResult();
            }
        });

        connection.Reconnecting += error =>
        {
            logger.LogError(error, "SignalR reconnecting...");
            return Task.CompletedTask;
        };

        connection.Reconnected += connectionId =>
        {
            logger.LogInformation("SignalR reconnected. ConnectionId={ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        connection.Closed += async error =>
        {
            if (stoppingToken.IsCancellationRequested || done.Task.IsCompleted)
            {
                logger.LogInformation("SignalR connection closed.");
                return;
            }

            logger.LogError(error, "SignalR connection closed. Waiting before reconnect attempt...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_settings.ReconnectDelay, stoppingToken);
                    await connection.StartAsync(stoppingToken);
                    logger.LogInformation("SignalR reconnect attempt succeeded.");
                    return;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "SignalR reconnect attempt failed.");
                }
            }
        };

        try
        {
            await ConnectWithRetryAsync(connection, stoppingToken);

            logger.LogInformation(
                "Connected to {HubUrl}, waiting for {BatchCount} batches from event '{EventName}'.",
                _settings.HubUrl,
                _settings.MinBatchesToLog,
                _settings.EventName
            );

            await Task.WhenAny(done.Task, Task.Delay(Timeout.Infinite, stoppingToken));

            if (done.Task.IsCompletedSuccessfully)
            {
                logger.LogInformation("Received {BatchCount} batches successfully. Stopping client.", _settings.MinBatchesToLog);
                applicationLifetime.StopApplication();
            }
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    private async Task ConnectWithRetryAsync(HubConnection connection, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await connection.StartAsync(cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SignalR connect failed. Next retry in {Delay}.", _settings.ReconnectDelay);
            }

            await Task.Delay(_settings.ReconnectDelay, cancellationToken);
        }
    }

    private static SatelliteSignalRClientSettings GetSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection("SignalRClient").Get<SatelliteSignalRClientSettings>()
                       ?? throw new InvalidOperationException(
                           "Configuration section 'SignalRClient' is missing or invalid."
                       );

        if (!Uri.TryCreate(settings.HubUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Configuration section 'SignalRClient' is invalid: 'HubUrl' must be absolute.");
        }

        if (settings.MinBatchesToLog <= 0)
        {
            throw new InvalidOperationException(
                "Configuration section 'SignalRClient' is invalid: 'MinBatchesToLog' must be greater than 0."
            );
        }

        if (settings.ReconnectDelay <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Configuration section 'SignalRClient' is invalid: 'ReconnectDelay' must be greater than 00:00:00."
            );
        }

        return settings;
    }
}

internal sealed record SatelliteSignalRClientSettings
{
    public required string HubUrl { get; init; }
    public string EventName { get; init; } = "satellitePositions";
    public int MinBatchesToLog { get; init; } = 10;
    public TimeSpan ReconnectDelay { get; init; } = TimeSpan.FromSeconds(5);
}
