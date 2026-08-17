using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using OrbitWatcher.Contracts;
using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.Services;
using OrbitWatcher.SignalR;
using OrbitWatcher.Storage;

namespace OrbitWatcher.HostedServices;

public sealed class SatelliteStreamerHostedService(
    ISatelliteStorage satelliteStorage,
    IOptions<SatelliteStreamingSettings> options,
    IHubContext<SatellitesHub> hubContext,
    ILogger<SatelliteStreamerHostedService> logger
) : BackgroundService
{
    private readonly SatelliteStreamingSettings _settings = options.Value;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Satellite streamer background service is starting.");

        using var timer = new PeriodicTimer(_settings.ExecuteInterval);

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
                        var eci = satellite.Predict(timestampUtc);
                        var geodetic = eci.ToGeodetic();
                        positions.Add(
                            new SatellitePositionDto(
                                satellite.Tle.NoradNumber,
                                satellite.Name,
                                timestampUtc,
                                geodetic.Latitude.Degrees,
                                geodetic.Longitude.Degrees,
                                geodetic.Altitude,
                                eci.Velocity.X,
                                eci.Velocity.Y,
                                eci.Velocity.Z,
                                eci.Velocity.Length,
                                satellite.Orbit.Period,
                                satellite.Orbit.Inclination.Degrees,
                                satellite.Orbit.Eccentricity,
                                satellite.Orbit.Perigee,
                                satellite.Orbit.Apogee,
                                ConstellationResolver.Resolve(satellite.Name)
                            )
                        );
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to calculate position for satellite '{NoradCatId}'.", satellite.Tle.NoradNumber);
                    }
                }

                await hubContext.Clients.All.SendAsync(
                    HubConstants.SatellitePositionsEventName,
                    positions,
                    stoppingToken
                );

                logger.LogDebug(
                    "Broadcasted {PositionsCount} satellite positions to hub '{HubRoute}'.",
                    positions.Count,
                    HubConstants.Route
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
