namespace OrbitWatcher.Celestrak;

public interface ICelestrakOmmClient
{
    Task<IReadOnlyCollection<OmmRecord>> DownloadOmmsAsync(CancellationToken cancellationToken = default);
}
