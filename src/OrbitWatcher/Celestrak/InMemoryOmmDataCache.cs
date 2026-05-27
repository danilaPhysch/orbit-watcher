namespace OrbitWatcher.Celestrak;

public sealed class InMemoryOmmDataCache(ICelestrakOmmClient client) : IOmmDataCache, IDisposable
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private string? _rawOmm;
    private DateTimeOffset? _lastRefreshUtc;

    public string? RawOmm => _rawOmm;
    public DateTimeOffset? LastRefreshUtc => _lastRefreshUtc;

    public async Task<string> RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var omm = await client.DownloadOmmAsync(cancellationToken);
            _rawOmm = omm;
            _lastRefreshUtc = DateTimeOffset.UtcNow;
            return omm;
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
