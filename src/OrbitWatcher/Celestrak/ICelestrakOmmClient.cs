namespace OrbitWatcher.Celestrak;

public interface ICelestrakOmmClient
{
    Task<string> DownloadOmmAsync(CancellationToken cancellationToken = default);
}
