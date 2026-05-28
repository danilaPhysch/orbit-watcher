namespace OrbitWatcher.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var ommLoadingSettings = configuration.GetSettings<OmmLoadingSettings>("OmmLoading");
        var celestrackSettings = configuration.GetSettings<CelestrackSettings>("Celestrack");

        services.AddSingleton(ommLoadingSettings);
        services.AddSingleton(celestrackSettings);
    }
}