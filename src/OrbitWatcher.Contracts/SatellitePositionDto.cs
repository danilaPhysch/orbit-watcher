namespace OrbitWatcher.Contracts;

public sealed record SatellitePositionDto(
    uint NoradCatId,
    string Name,
    DateTime TimestampUtc,
    double Lat,
    double Lon,
    double AltKm,
    double VelocityXKmS,
    double VelocityYKmS,
    double VelocityZKmS,
    double SpeedKmS,
    double OrbitalPeriodMin,
    double InclinationDeg,
    double Eccentricity,
    double PerigeeKm,
    double ApogeeKm,
    string Constellation
);
