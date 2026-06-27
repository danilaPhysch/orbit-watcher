namespace OrbitWatcher.Contracts;

public sealed record GroundTrackPointDto(
    DateTime TimestampUtc,
    double Lat,
    double Lon,
    double AltKm
);

public sealed record GroundTrackDto(
    uint NoradCatId,
    DateTime GeneratedAtUtc,
    IReadOnlyCollection<IReadOnlyCollection<GroundTrackPointDto>> PastTrack,
    IReadOnlyCollection<IReadOnlyCollection<GroundTrackPointDto>> FutureTrack
);
