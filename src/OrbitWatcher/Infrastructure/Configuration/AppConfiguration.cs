namespace OrbitWatcher.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var ommLoadingSettings = ValidateOmmLoadingSettings(configuration.GetSettings<OmmLoadingSettings>("OmmLoading"));
        var celestrackSettings = ValidateCelestrackSettings(configuration.GetSettings<CelestrackSettings>("Celestrack"));

        services.AddSingleton(ommLoadingSettings);
        services.AddSingleton(celestrackSettings);
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
}
