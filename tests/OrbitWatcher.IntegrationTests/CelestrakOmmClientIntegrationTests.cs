using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrbitWatcher.Celestrak;

namespace OrbitWatcher.IntegrationTests;

public class CelestrakOmmClientIntegrationTests
{
    [Fact]
    public async Task RefreshAsync_WithRealCelestrakApi_DownloadsNonEmptyOmmPayload()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var services = new ServiceCollection();
        services.AddCelestrakOmm(configuration);

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<IOmmDataCache>();

        IReadOnlyCollection<OmmRecord> omms;
        try
        {
            omms = await cache.RefreshAsync();
        }
        catch (HttpRequestException ex) when (
            (ex.StatusCode == System.Net.HttpStatusCode.Forbidden &&
             ex.Message.Contains("updated once every 2 hours", StringComparison.OrdinalIgnoreCase)) ||
            ex.Message.Contains("Resource temporarily unavailable", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Assert.NotEmpty(omms);
        var sample = omms.First();
        Assert.False(string.IsNullOrWhiteSpace(sample.ObjectName));
        Assert.False(string.IsNullOrWhiteSpace(sample.ObjectId));
        Assert.NotNull(sample.Epoch);
        Assert.True(sample.NoradCatId > 0);
        Assert.Equal(omms, cache.Omms);
        Assert.NotNull(cache.LastRefreshUtc);
    }
}
