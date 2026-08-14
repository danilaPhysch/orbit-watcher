namespace OrbitWatcher.Contracts;

/// <summary>
/// Shared constants for the SignalR satellites hub,
/// used by both server and client projects.
/// </summary>
public static class HubConstants
{
    public const string Route = "/hubs/satellites";
    public const string SatellitePositionsEventName = "satellitePositions";
    public const string GetGroundTrackMethodName = "GetGroundTrack";
}
