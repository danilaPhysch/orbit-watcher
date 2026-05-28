namespace OrbitWatcher.Contracts;

public sealed record SatellitePositionDto(
    uint NoradCatId,
    string Name,
    DateTime TimestampUtc,
    double Lat,
    double Lon,
    double AltKm
);
