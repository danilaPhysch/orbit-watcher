namespace OrbitWatcher.SignalRClient;

internal sealed record SatelliteSignalRClientSettings
{
    public required string HubUrl { get; init; }
    public string EventName { get; init; } = "satellitePositions";
    public int MinBatchesToLog { get; init; } = 10;
    public TimeSpan ReconnectDelay { get; init; } = TimeSpan.FromSeconds(5);
}
