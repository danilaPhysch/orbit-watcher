using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrbitWatcher.Celestrak;

namespace OrbitWatcher.IntegrationTests;

public class CelestrakOmmClientIntegrationTests
{
    [Fact(Skip = "Requires outbound network access to celestrak.org.")]
    public async Task RefreshAsync_WithRealCelestrakApi_DownloadsNonEmptyOmmPayload()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var services = new ServiceCollection();
        services.AddCelestrakOmm(configuration);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IOmmDataCache>();

        var payload = await cache.RefreshAsync();

        Assert.False(string.IsNullOrWhiteSpace(payload));
        Assert.True(HasExpectedOmmMarker(payload));
        Assert.Equal(payload, cache.RawOmm);
        Assert.NotNull(cache.LastRefreshUtc);
    }

    private static bool HasExpectedOmmMarker(string payload)
    {
        var markers = new[]
        {
            "<omm",
            "<segment",
            "CCSDS_OMM_VERS"
        };

        return markers.Any(marker => payload.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
