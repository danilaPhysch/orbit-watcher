using Microsoft.Extensions.Options;
using OrbitWatcher.Infrastructure.Configuration;
using SGPdotNET.Parsers;

namespace OrbitWatcher.Services;

public sealed class CelestrackClient(HttpClient httpClient, IOptions<CelestrackSettings> options, ILogger<CelestrackClient> logger)
    : ICelestrackClient
{
    private static readonly OmmJsonParser _parser = new();
    private readonly CelestrackSettings _settings = options.Value;

    public async Task<IReadOnlyCollection<OmmData>> DownloadOmm(CancellationToken cancellationToken)
    {
        List<OmmData> output = [];

        foreach (var relativeUri in _settings.RelativeUris)
        {
            var uri = new Uri(_settings.BaseUri, relativeUri);

            using var response = await httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Failed to download OMM data from {Uri}. Status code: {StatusCode}. Response body: {ResponseBody}",
                    uri,
                    response.StatusCode,
                    errorBody
                );
                continue;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            output.AddRange(_parser.Parse(content));
        }

        return output;
    }
}
