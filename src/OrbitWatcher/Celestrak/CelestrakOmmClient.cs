using Microsoft.Extensions.Options;
using System.Text.Json;

namespace OrbitWatcher.Celestrak;

public sealed class CelestrakOmmClient(HttpClient httpClient, IOptions<CelestrakOmmOptions> options) : ICelestrakOmmClient
{
    public async Task<IReadOnlyCollection<OmmRecord>> DownloadOmmsAsync(CancellationToken cancellationToken = default)
    {
        var baseUri = new Uri(options.Value.BaseUrl);
        var uri = new Uri(baseUri, options.Value.RelativeUri);
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
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

        var records = JsonSerializer.Deserialize<List<OmmRecord>>(content);
        if (records is null || records.Count == 0)
        {
            throw new InvalidOperationException("Celestrak returned no OMM records.");
        }

        return records;
    }
}
