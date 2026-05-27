using Microsoft.Extensions.Options;

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
        });

        services.AddSingleton<IOmmDataCache, InMemoryOmmDataCache>();

        return services;
    }
}
