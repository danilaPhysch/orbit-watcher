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

    /// <summary>
    /// Subscribes the client to the specified constellation groups.
    /// Unsubscribes from any previously subscribed groups not in the new list.
    /// </summary>
    public async Task SetSubscriptions(string[] constellations)
    {
        var currentSubs = Context.Items["Subscriptions"] as HashSet<string> ?? [];
        var newSubs = new HashSet<string>(constellations, StringComparer.OrdinalIgnoreCase);

        var toRemove = currentSubs.Except(newSubs).ToList();
        var toAdd = newSubs.Except(currentSubs).ToList();

        foreach (var group in toRemove)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Constellation_{group}");
        }

        foreach (var group in toAdd)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Constellation_{group}");
        }

        Context.Items["Subscriptions"] = newSubs;
    }
}
