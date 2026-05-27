using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace OrbitWatcher.Celestrak;

public static class CelestrakServiceCollectionExtensions
{
    public static IServiceCollection AddCelestrakOmm(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<CelestrakOmmOptions>()
            .Bind(configuration.GetSection(CelestrakOmmOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddHttpClient<ICelestrakOmmClient, CelestrakOmmClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CelestrakOmmOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("orbit-watcher/1.0 (+https://github.com/danilaPhysch/orbit-watcher)");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        });

        services.AddSingleton<IOmmDataCache, InMemoryOmmDataCache>();

        return services;
    }
}
