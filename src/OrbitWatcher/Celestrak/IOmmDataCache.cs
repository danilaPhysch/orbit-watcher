namespace OrbitWatcher.Celestrak;

public interface IOmmDataCache
{
    IReadOnlyCollection<OmmRecord> Omms { get; }
    DateTimeOffset? LastRefreshUtc { get; }
    Task<IReadOnlyCollection<OmmRecord>> RefreshAsync(CancellationToken cancellationToken = default);
}
