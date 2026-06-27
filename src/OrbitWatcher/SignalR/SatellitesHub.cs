using Microsoft.AspNetCore.SignalR;
using OrbitWatcher.Contracts;
using OrbitWatcher.Storage;
using OrbitWatcher.Infrastructure.Configuration;

namespace OrbitWatcher.SignalR;

public sealed class SatellitesHub(
    SatelliteStorage satelliteStorage,
    GroundTrackSettings settings,
    ILogger<SatellitesHub> logger
) : Hub
{
    public const string Route = "/hubs/satellites";
    public const string SatellitePositionsEventName = "satellitePositions";

    public GroundTrackDto? GetGroundTrack(uint noradCatId)
    {
        if (!satelliteStorage.TryGetByNoradCatId(noradCatId, out var satellite) || satellite is null)
        {
            logger.LogWarning("Ground track requested for non-existent satellite NORAD ID {NoradCatId}", noradCatId);
            return null;
        }

        try
        {
            var now = DateTime.UtcNow;
            var periodMinutes = satellite.Orbit.Period;
            var halfOrbitMinutes = periodMinutes * settings.HalfOrbitFraction;
            var halfOrbitSpan = TimeSpan.FromMinutes(halfOrbitMinutes);

            var pastPoints = CalculatePoints(satellite, now - halfOrbitSpan, now, TimeSpan.FromSeconds(settings.StepSeconds));
            var futurePoints = CalculatePoints(satellite, now, now + halfOrbitSpan, TimeSpan.FromSeconds(settings.StepSeconds));

            var pastSegments = SegmentTrack(pastPoints);
            var futureSegments = SegmentTrack(futurePoints);

            return new GroundTrackDto(
                noradCatId,
                now,
                pastSegments,
                futureSegments
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate ground track for satellite {NoradCatId}", noradCatId);
            return null;
        }
    }

    private static List<GroundTrackPointDto> CalculatePoints(
        SGPdotNET.Observation.Satellite satellite,
        DateTime start,
        DateTime end,
        TimeSpan step)
    {
        var points = new List<GroundTrackPointDto>();
        for (var t = start; t <= end; t = t.Add(step))
        {
            try
            {
                var geodetic = satellite.Predict(t).ToGeodetic();
                points.Add(new GroundTrackPointDto(
                    t,
                    geodetic.Latitude.Degrees,
                    geodetic.Longitude.Degrees,
                    geodetic.Altitude
                ));
            }
            catch
            {
                // Skip points that fail to propagate (e.g. decayed orbit)
            }
        }

        // Always make sure the end point is included if we missed it due to step size
        if (points.Count > 0 && points[^1].TimestampUtc != end)
        {
            try
            {
                var geodetic = satellite.Predict(end).ToGeodetic();
                points.Add(new GroundTrackPointDto(
                    end,
                    geodetic.Latitude.Degrees,
                    geodetic.Longitude.Degrees,
                    geodetic.Altitude
                ));
            }
            catch
            {
                // Skip if end point fails
            }
        }

        return points;
    }

    private static List<List<GroundTrackPointDto>> SegmentTrack(List<GroundTrackPointDto> points)
    {
        var segments = new List<List<GroundTrackPointDto>>();
        if (points.Count == 0)
        {
            return segments;
        }

        var currentSegment = new List<GroundTrackPointDto> { points[0] };
        for (var i = 1; i < points.Count; i++)
        {
            var prev = points[i - 1];
            var curr = points[i];

            // If we cross the antimeridian, there's a big jump in longitude
            // e.g. from 179 to -179 is a difference of 358 degrees
            var lonDiff = Math.Abs(curr.Lon - prev.Lon);
            if (lonDiff > 180.0)
            {
                segments.Add(currentSegment);
                currentSegment = new List<GroundTrackPointDto>();
            }

            currentSegment.Add(curr);
        }

        if (currentSegment.Count > 0)
        {
            segments.Add(currentSegment);
        }

        return segments;
    }
}
