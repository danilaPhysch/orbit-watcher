using Microsoft.Extensions.Options;

namespace OrbitWatcher.Celestrak;

public sealed class CelestrakOmmClient(HttpClient httpClient, IOptions<CelestrakOmmOptions> options) : ICelestrakOmmClient
{
    public async Task<string> DownloadOmmAsync(CancellationToken cancellationToken = default)
    {
        var endpointPath = options.Value.EndpointPath;

        using var response = await httpClient.GetAsync(endpointPath, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Celestrak OMM request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}). Response: {errorBody}",
                null,
                response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Celestrak returned an empty OMM payload.");
        }

        return content;
    }
}
