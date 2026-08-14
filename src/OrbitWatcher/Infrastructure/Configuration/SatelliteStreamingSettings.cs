namespace OrbitWatcher.Infrastructure.Configuration;

public sealed class SatelliteStreamingSettings
{
    public required TimeSpan ExecuteInterval { get; init; }
}
