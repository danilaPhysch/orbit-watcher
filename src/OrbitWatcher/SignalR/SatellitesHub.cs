using Microsoft.AspNetCore.SignalR;

namespace OrbitWatcher.SignalR;

public sealed class SatellitesHub : Hub
{
    public const string Route = "/hubs/satellites";
    public const string SatellitePositionsEventName = "satellitePositions";
}
