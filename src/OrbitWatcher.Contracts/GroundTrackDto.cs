namespace OrbitWatcher.Contracts;

/// <summary>
/// Ground track response containing the track split into segments.
/// Each segment is a contiguous set of points without anti-meridian crossing,
/// so the client can draw each segment as a separate polyline.
/// </summary>
public sealed record GroundTrackDto(
    uint NoradCatId,
    DateTime GeneratedAtUtc,
    IReadOnlyList<IReadOnlyList<GroundTrackPointDto>> Segments
);
