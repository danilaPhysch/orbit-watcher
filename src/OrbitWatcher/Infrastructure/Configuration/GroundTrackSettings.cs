namespace OrbitWatcher.Infrastructure.Configuration;

public class GroundTrackSettings
{
    /// <summary>
    /// Fraction of the orbital period to show before and after the current time.
    /// Default is 0.5 (half orbit in each direction).
    /// </summary>
    public double HalfOrbitFraction { get; init; } = 0.5;

    /// <summary>
    /// Time step in seconds between consecutive ground track points.
    /// </summary>
    public int StepSeconds { get; init; } = 15;
}
