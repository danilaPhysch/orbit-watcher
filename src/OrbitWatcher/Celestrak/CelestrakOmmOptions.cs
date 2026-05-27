using System.ComponentModel.DataAnnotations;

namespace OrbitWatcher.Celestrak;

public sealed class CelestrakOmmOptions
{
    public const string SectionName = "CelestrakOmm";

    [Required]
    public string BaseUrl { get; set; } = "https://celestrak.org";

    [Required]
    public string EndpointPath { get; set; } = "NORAD/elements/gp.php?GROUP=active&FORMAT=XML";

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}
