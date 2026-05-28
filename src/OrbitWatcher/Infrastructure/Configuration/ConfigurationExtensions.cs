namespace OrbitWatcher.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static T GetSettings<T>(this IConfiguration configuration, string section)
        where T : class =>
        configuration.GetSection(section).Get<T>()
        ?? throw new InvalidOperationException($"Configuration section '{section}' not found or is empty.");
}
