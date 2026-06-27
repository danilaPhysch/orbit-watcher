namespace OrbitWatcher.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var ommLoadingSettings = ValidateOmmLoadingSettings(configuration.GetSettings<OmmLoadingSettings>("OmmLoading"));
        var celestrackSettings = ValidateCelestrackSettings(configuration.GetSettings<CelestrackSettings>("Celestrack"));
        var satelliteStreamingSettings = ValidateSatelliteStreamingSettings(
            configuration.GetSettings<SatelliteStreamingSettings>("SatelliteStreaming")
        );
        var groundTrackSettings = ValidateGroundTrackSettings(
            configuration.GetSettings<GroundTrackSettings>("GroundTrack")
        );

        services.AddSingleton(ommLoadingSettings);
        services.AddSingleton(celestrackSettings);
        services.AddSingleton(satelliteStreamingSettings);
        services.AddSingleton(groundTrackSettings);
    }

    private static GroundTrackSettings ValidateGroundTrackSettings(GroundTrackSettings? settings)
    {
        if (settings is null)
        {
            throw new InvalidOperationException("Configuration section 'GroundTrack' is missing or invalid.");
        }

        if (settings.HalfOrbitFraction <= 0.0)
        {
            throw new InvalidOperationException(
                "Configuration section 'GroundTrack' is invalid: 'HalfOrbitFraction' must be greater than 0."
            );
        }

        if (settings.StepSeconds <= 0)
        {
            throw new InvalidOperationException(
                "Configuration section 'GroundTrack' is invalid: 'StepSeconds' must be greater than 0."
            );
        }

        if (settings.UpdateIntervalSeconds <= 0)
        {
            throw new InvalidOperationException(
                "Configuration section 'GroundTrack' is invalid: 'UpdateIntervalSeconds' must be greater than 0."
            );
        }

        return settings;
    }

    private static OmmLoadingSettings ValidateOmmLoadingSettings(OmmLoadingSettings? settings)
    {
        if (settings is null)
        {
            throw new InvalidOperationException("Configuration section 'OmmLoading' is missing or invalid.");
        }

        if (settings.ExecuteInterval <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Configuration section 'OmmLoading' is invalid: 'ExecuteInterval' must be greater than 00:00:00."
            );
        }

        return settings;
    }

    private static CelestrackSettings ValidateCelestrackSettings(CelestrackSettings? settings)
    {
        if (settings is null)
        {
            throw new InvalidOperationException("Configuration section 'Celestrack' is missing or invalid.");
        }

        if (settings.RelativeUris is null || !settings.RelativeUris.Any())
        {
            throw new InvalidOperationException(
                "Configuration section 'Celestrack' is invalid: 'RelativeUris' must contain at least one value."
            );
        }

        return settings;
    }

    private static SatelliteStreamingSettings ValidateSatelliteStreamingSettings(SatelliteStreamingSettings? settings)
    {
        if (settings is null)
        {
            throw new InvalidOperationException("Configuration section 'SatelliteStreaming' is missing or invalid.");
        }

        if (settings.ExecuteInterval <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Configuration section 'SatelliteStreaming' is invalid: 'ExecuteInterval' must be greater than 00:00:00."
            );
        }

        return settings;
    }
}
