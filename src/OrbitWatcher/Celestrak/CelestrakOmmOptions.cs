using System.ComponentModel.DataAnnotations;

namespace OrbitWatcher.Celestrak;

public sealed class CelestrakOmmOptions
{
    public const string SectionName = "CelestrakOmm";

    [Required]
    public string BaseUrl { get; set; } = "https://celestrak.org";

    [Required]
    public string RelativeUri { get; set; } = "NORAD/elements/gp.php?GROUP=active&FORMAT=JSON";

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}
