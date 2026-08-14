using Microsoft.Extensions.Options;
using OrbitWatcher.Contracts;
using OrbitWatcher.Infrastructure.Configuration;
using OrbitWatcher.Storage;

namespace OrbitWatcher.Services;

public sealed class GroundTrackService(
    SatelliteStorage satelliteStorage,
    IOptions<GroundTrackSettings> options,
    ILogger<GroundTrackService> logger
)
{
    private readonly GroundTrackSettings _settings = options.Value;
    private const double MinutesPerDay = 1440.0;

    /// <summary>
    /// Longitude difference threshold (in degrees) between two consecutive points
    /// that indicates an anti-meridian crossing.
    /// </summary>
    private const double AntiMeridianThresholdDeg = 270.0;

    public GroundTrackDto? Calculate(uint noradCatId, DateTime timestampUtc)
    {
        if (!satelliteStorage.TryGetByNoradCatId(noradCatId, out var satellite) || satellite is null)
        {
            logger.LogWarning("Satellite with NORAD catalog ID {NoradCatId} not found.", noradCatId);
            return null;
        }

        var meanMotionRevPerDay = satellite.Tle.MeanMotionRevPerDay;
        if (meanMotionRevPerDay <= 0)
        {
            logger.LogWarning(
                "Satellite {NoradCatId} has invalid mean motion ({MeanMotion}). Cannot calculate ground track.",
                noradCatId, meanMotionRevPerDay
            );
            return null;
        }

        var orbitalPeriodMinutes = MinutesPerDay / meanMotionRevPerDay;
        var halfWindowMinutes = orbitalPeriodMinutes * _settings.HalfOrbitFraction;

        var startTime = timestampUtc.AddMinutes(-halfWindowMinutes);
        var endTime = timestampUtc.AddMinutes(halfWindowMinutes);
        var step = TimeSpan.FromSeconds(_settings.StepSeconds);

        var allPoints = new List<GroundTrackPointDto>();

        for (var t = startTime; t <= endTime; t += step)
        {
            try
            {
                var geodetic = satellite.Predict(t).ToGeodetic();
                allPoints.Add(new GroundTrackPointDto(
                    t,
                    geodetic.Latitude.Degrees,
                    geodetic.Longitude.Degrees,
                    geodetic.Altitude
                ));
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to propagate position for satellite {NoradCatId} at {Timestamp}.",
                    noradCatId, t
                );
            }
        }

        var segments = SplitByAntiMeridian(allPoints);

        return new GroundTrackDto(noradCatId, timestampUtc, segments);
    }

    /// <summary>
    /// Splits a sequence of ground track points into segments whenever the longitude
    /// jumps across the anti-meridian (±180°). This prevents the "line across the world" artifact.
    /// </summary>
    private static List<IReadOnlyList<GroundTrackPointDto>> SplitByAntiMeridian(
        List<GroundTrackPointDto> points)
    {
        if (points.Count == 0)
        {
            return [];
        }

        var segments = new List<IReadOnlyList<GroundTrackPointDto>>();
        var currentSegment = new List<GroundTrackPointDto> { points[0] };

        for (var i = 1; i < points.Count; i++)
        {
            var prevLon = points[i - 1].Lon;
            var currLon = points[i].Lon;

            if (Math.Abs(currLon - prevLon) > AntiMeridianThresholdDeg)
            {
                // Anti-meridian crossing detected — start a new segment
                segments.Add(currentSegment);
                currentSegment = [];
            }

            currentSegment.Add(points[i]);
        }

        if (currentSegment.Count > 0)
        {
            segments.Add(currentSegment);
        }

        return segments;
    }
}
