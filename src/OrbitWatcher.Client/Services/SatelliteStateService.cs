using OrbitWatcher.Contracts;

namespace OrbitWatcher.Client.Services;

/// <summary>
/// Centralized satellite state management shared across pages.
/// Subscribes to <see cref="SatellitePositionsStream"/> once and
/// exposes the latest positions via a dictionary snapshot.
/// </summary>
public sealed class SatelliteStateService : IDisposable
{
    private readonly SatellitePositionsStream _stream;
    private readonly Dictionary<uint, SatellitePositionDto> _satellites = [];
    private readonly HashSet<string> _subscriptions = [
        "GPS", "GLONASS", "Galileo", "BeiDou", "QZSS", "NavIC", "SBAS", "Other"
    ];

    public SatelliteStateService(SatellitePositionsStream stream)
    {
        _stream = stream;
        _stream.PositionsReceived += OnPositionsReceived;
        _stream.StatusChanged += OnStatusChanged;
    }

    public event Action? StateChanged;

    public SatellitePositionsStream Stream => _stream;

    public IReadOnlyDictionary<uint, SatellitePositionDto> Satellites => _satellites;
    public IReadOnlySet<string> Subscriptions => _subscriptions;

    public async Task EnsureStartedAsync(CancellationToken cancellationToken = default)
    {
        await _stream.SetSubscriptionsAsync(_subscriptions, cancellationToken);
        await _stream.StartAsync(cancellationToken);
    }

    public async Task UpdateSubscriptionsAsync(IEnumerable<string> newSubscriptions)
    {
        _subscriptions.Clear();
        foreach (var sub in newSubscriptions)
        {
            _subscriptions.Add(sub);
        }

        // Remove satellites that are no longer subscribed
        var toRemove = _satellites.Values
            .Where(s => !_subscriptions.Contains(s.Constellation))
            .Select(s => s.NoradCatId)
            .ToList();

        foreach (var id in toRemove)
        {
            _satellites.Remove(id);
        }

        await _stream.SetSubscriptionsAsync(_subscriptions);
        StateChanged?.Invoke();
    }

    private void OnPositionsReceived(IReadOnlyCollection<SatellitePositionDto> positions)
    {
        foreach (var satellite in positions)
        {
            _satellites[satellite.NoradCatId] = satellite;
        }

        StateChanged?.Invoke();
    }

    private void OnStatusChanged() => StateChanged?.Invoke();

    public void Dispose()
    {
        _stream.PositionsReceived -= OnPositionsReceived;
        _stream.StatusChanged -= OnStatusChanged;
    }
}
