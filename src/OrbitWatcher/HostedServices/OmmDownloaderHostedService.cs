using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.Services;
using OrbitWatcher.Storage;
using SGPdotNET.Observation;

namespace OrbitWatcher.HostedServices;

public class OmmDownloaderHostedService(
    ICelestrackClient celestrackClient,
    OmmLoadingSettings settings,
    SatelliteStorage satelliteStorage,
    ILogger<OmmDownloaderHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OMM downloader background service is starting.");

        using var timer = new PeriodicTimer(settings.ExecuteInterval);

        do
        {
            try
            {
                logger.LogInformation("Starting download of OMM data...");

                var ommData = await celestrackClient.DownloadOmm(stoppingToken);

                var satellites = ommData.Select(omm => new Satellite(omm)).ToList();

                satelliteStorage.AddOrUpdate(satellites);

                logger.LogInformation("Successfully updated {Count} satellites in memory.", ommData.Count);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching OMM data.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}