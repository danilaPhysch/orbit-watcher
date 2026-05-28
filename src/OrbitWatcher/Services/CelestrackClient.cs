using OrbitWatcher.Infrastructure.Configuration;
using SGPdotNET.Parsers;

namespace OrbitWatcher.Services;

public sealed class CelestrackClient(HttpClient httpClient, CelestrackSettings celestrackSettings, ILogger<CelestrackClient> logger):ICelestrackClient
{
    public async Task<IReadOnlyCollection<OmmData>> DownloadOmm(CancellationToken cancellationToken)
    {
        List<OmmData> output = [];
        foreach (var relativeUri in celestrackSettings.RelativeUris)
        {
            var uri = new Uri(celestrackSettings.BaseUri, relativeUri);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("Failed to download OMM data from {Uri}. Status code: {StatusCode}. Response body: {ResponseBody}", uri, response.StatusCode, errorBody);
                continue;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            output.AddRange(new OmmJsonParser().Parse(content));
        }

        return output;
    }
}