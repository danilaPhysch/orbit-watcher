using SGPdotNET.Parsers;

namespace OrbitWatcher.Services;

public interface ICelestrackClient
{
    Task<IReadOnlyCollection<OmmData>> DownloadOmm(CancellationToken cancellationToken);
}