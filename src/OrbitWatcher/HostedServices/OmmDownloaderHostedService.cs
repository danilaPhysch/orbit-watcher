using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.Services;
using OrbitWatcher.Storage;
using SGPdotNET.Observation;

namespace OrbitWatcher.HostedServices;

public sealed class OmmDownloaderHostedService(
    ICelestrackClient celestrackClient,
    OmmLoadingSettings settings,
    SatelliteStorage satelliteStorage,
    ILogger<OmmDownloaderHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OMM downloader background service is starting.");

        using var timer = new PeriodicTimer(settings.ExecuteInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    logger.LogInformation("Starting download of OMM data...");

                    var ommData = await celestrackClient.DownloadOmm(stoppingToken);

                    if (ommData.Count == 0)
                    {
                        logger.LogWarning("OMM download completed but returned 0 items; keeping previous in-memory snapshot.");
                        continue;
                    }

                    var satellites = ommData.Select(omm => new Satellite(omm)).ToList();

                    satelliteStorage.ReplaceAll(satellites);

                    logger.LogInformation(
                        "Successfully updated in-memory satellite snapshot. Satellites: {Count}",
                        satelliteStorage.Count);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Normal shutdown.
                    logger.LogInformation("OMM downloader background service is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while fetching OMM data. Keeping previous in-memory snapshot.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("OMM downloader background service is stopping.");
        }
    }
}
