namespace OrbitWatcher.Contracts;

public sealed record GroundTrackDto(
    uint NoradCatId,
    DateTime GeneratedAtUtc,
    IReadOnlyCollection<GroundTrackPointDto> Points
);
