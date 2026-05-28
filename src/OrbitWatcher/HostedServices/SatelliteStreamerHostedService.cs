using Microsoft.AspNetCore.SignalR;
using OrbitWatcher.Contracts;
using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.SignalR;
using OrbitWatcher.Storage;

namespace OrbitWatcher.HostedServices;

public sealed class SatelliteStreamerHostedService(
    SatelliteStorage satelliteStorage,
    SatelliteStreamingSettings settings,
    IHubContext<SatellitesHub> hubContext,
    ILogger<SatelliteStreamerHostedService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Satellite streamer background service is starting.");

        using var timer = new PeriodicTimer(settings.ExecuteInterval);

        do
        {
            try
            {
                var timestampUtc = DateTime.UtcNow;
                var snapshot = satelliteStorage.GetAllSnapshot();
                var positions = new List<SatellitePositionDto>(snapshot.Count);

                foreach (var satellite in snapshot)
                {
                    try
                    {
                        var geodetic = satellite.Predict(timestampUtc).ToGeodetic();
                        positions.Add(
                            new SatellitePositionDto(
                                satellite.Tle.NoradNumber,
                                satellite.Name,
                                timestampUtc,
                                geodetic.Latitude.Degrees,
                                geodetic.Longitude.Degrees,
                                geodetic.Altitude
                            )
                        );
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to calculate position for satellite '{NoradCatId}'.", satellite.Tle.NoradNumber);
                    }
                }

                await hubContext.Clients.All.SendAsync(
                    SatellitesHub.SatellitePositionsEventName,
                    positions,
                    stoppingToken
                );

                logger.LogDebug(
                    "Broadcasted {PositionsCount} satellite positions to hub '{HubRoute}'.",
                    positions.Count,
                    SatellitesHub.Route
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Satellite streamer background service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while broadcasting satellite positions.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
