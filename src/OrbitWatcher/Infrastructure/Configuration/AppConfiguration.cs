namespace OrbitWatcher.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var ommLoadingSettings = configuration.GetSettings<OmmLoadingSettings>("OmmLoading");
        var celestrackSettings = configuration.GetSettings<CelestrackSettings>("Celestrack");
        var satelliteStreamingSettings =
            configuration.GetSettings<SatelliteStreamingSettings>("SatelliteStreaming")
        ;
        var groundTrackSettings =
            configuration.GetSettings<GroundTrackSettings>("GroundTrack")
        ;

        services.AddSingleton(ommLoadingSettings);
        services.AddSingleton(celestrackSettings);
        services.AddSingleton(satelliteStreamingSettings);
        services.AddSingleton(groundTrackSettings);
    }
}
