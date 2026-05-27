using Microsoft.Extensions.Options;

namespace OrbitWatcher.Celestrak;

public sealed class CelestrakOmmClient(HttpClient httpClient, IOptions<CelestrakOmmOptions> options) : ICelestrakOmmClient
{
    public async Task<string> DownloadOmmAsync(CancellationToken cancellationToken = default)
    {
        var endpointPath = options.Value.EndpointPath;

        using var response = await httpClient.GetAsync(endpointPath, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Celestrak returned an empty OMM payload.");
        }

        return content;
    }
}
