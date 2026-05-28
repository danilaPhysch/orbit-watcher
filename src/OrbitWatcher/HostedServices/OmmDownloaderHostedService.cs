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

        do
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

                var satellitesByNoradCatId = new Dictionary<uint, Satellite>(ommData.Count);
                foreach (var omm in ommData)
                {
                    if (satellitesByNoradCatId.ContainsKey(omm.NoradCatID))
                    {
                        logger.LogWarning(
                            "OMM download contains duplicate NORAD catalog ID '{NoradCatId}'. Keeping the first occurrence and ignoring subsequent duplicates.",
                            omm.NoradCatID);
                        continue;
                    }

                    satellitesByNoradCatId.Add(omm.NoradCatID, new Satellite(omm));
                }

                satelliteStorage.ReplaceAll(satellitesByNoradCatId);

                logger.LogInformation(
                    "Successfully updated in-memory satellite snapshot. Satellites: {Count}",
                    satelliteStorage.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("OMM downloader background service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching OMM data. Keeping previous in-memory snapshot.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}