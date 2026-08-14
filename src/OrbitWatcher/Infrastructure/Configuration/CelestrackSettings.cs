namespace OrbitWatcher.Infrastructure.Configuration;

public sealed class CelestrackSettings
{
    public required Uri BaseUri { get; init; }
    public required string[] RelativeUris { get; init; }
}
