using Microsoft.AspNetCore.SignalR;
using OrbitWatcher.Contracts;
using OrbitWatcher.Services;

namespace OrbitWatcher.SignalR;

public sealed class SatellitesHub(GroundTrackService groundTrackService) : Hub
{
    /// <summary>
    /// Calculates and returns the ground track (±0.5 orbit) for the specified satellite.
    /// Called by clients via SignalR invocation.
    /// </summary>
    public GroundTrackDto? GetGroundTrack(uint noradCatId)
    {
        return groundTrackService.Calculate(noradCatId, DateTime.UtcNow);
    }
}
