namespace OrbitWatcher.Infrastructure.Configuration;

public sealed class GroundTrackSettings
{
    public double HalfOrbitFraction { get; init; } = 0.5;
    public int StepSeconds { get; init; } = 15;
    public int UpdateIntervalSeconds { get; init; } = 60;
}
