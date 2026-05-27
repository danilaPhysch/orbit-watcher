namespace OrbitWatcher.Celestrak;

public interface IOmmDataCache
{
    string? RawOmm { get; }
    DateTimeOffset? LastRefreshUtc { get; }
    Task<string> RefreshAsync(CancellationToken cancellationToken = default);
}
