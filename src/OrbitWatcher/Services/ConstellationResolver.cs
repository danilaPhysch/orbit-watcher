namespace OrbitWatcher.Services;

/// <summary>
/// Determines the GNSS constellation name from the satellite's display name.
/// </summary>
public static class ConstellationResolver
{
    public static string Resolve(string name)
    {
        if (name.StartsWith("GPS", StringComparison.OrdinalIgnoreCase))
            return "GPS";

        if (name.StartsWith("COSMOS", StringComparison.OrdinalIgnoreCase))
            return "GLONASS";

        if (name.StartsWith("GSAT0", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("GALILEO", StringComparison.OrdinalIgnoreCase))
            return "Galileo";

        if (name.StartsWith("BEIDOU", StringComparison.OrdinalIgnoreCase))
            return "BeiDou";

        if (name.StartsWith("QZS", StringComparison.OrdinalIgnoreCase))
            return "QZSS";

        if (name.StartsWith("IRNSS", StringComparison.OrdinalIgnoreCase))
            return "NavIC";

        if (name.StartsWith("INMARSAT", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("SES-", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("ASTRA", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("LUCH", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("EUTELSAT", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("GSAT-", StringComparison.OrdinalIgnoreCase))
            return "SBAS";

        return "Other";
    }
}
