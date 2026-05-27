namespace OrbitWatcher.Celestrak;

public sealed class InMemoryOmmDataCache(ICelestrakOmmClient client) : IOmmDataCache, IDisposable
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private IReadOnlyCollection<OmmRecord> _omms = [];
    private DateTimeOffset? _lastRefreshUtc;

    public IReadOnlyCollection<OmmRecord> Omms => _omms;
    public DateTimeOffset? LastRefreshUtc => _lastRefreshUtc;

    public async Task<IReadOnlyCollection<OmmRecord>> RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var omms = await client.DownloadOmmsAsync(cancellationToken);
            _omms = omms;
            _lastRefreshUtc = DateTimeOffset.UtcNow;
            return omms;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Dispose()
    {
        _refreshLock.Dispose();
    }
}
