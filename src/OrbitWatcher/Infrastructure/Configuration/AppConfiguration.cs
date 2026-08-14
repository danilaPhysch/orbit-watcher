namespace OrbitWatcher.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OmmLoadingSettings>(configuration.GetSection("OmmLoading"));
        services.Configure<CelestrackSettings>(configuration.GetSection("Celestrack"));
        services.Configure<SatelliteStreamingSettings>(configuration.GetSection("SatelliteStreaming"));
        services.Configure<GroundTrackSettings>(configuration.GetSection("GroundTrack"));
    }
}
