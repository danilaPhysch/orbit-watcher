using OrbitWatcher.Contracts;

namespace OrbitWatcher.Client.Services;

public sealed class SatelliteSignalRSettings
{
    public const string SectionName = "SignalRClient";

    public string? HubUrl { get; init; }
    public string HubBaseUrl { get; init; } = "http://localhost:5000";
    public string HubPath { get; init; } = HubConstants.Route;
    public string EventName { get; init; } = HubConstants.SatellitePositionsEventName;
}

