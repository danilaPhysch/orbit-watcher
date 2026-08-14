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

    public SatelliteStateService(SatellitePositionsStream stream)
    {
        _stream = stream;
        _stream.PositionsReceived += OnPositionsReceived;
        _stream.StatusChanged += OnStatusChanged;
    }

    public event Action? StateChanged;

    public SatellitePositionsStream Stream => _stream;

    public IReadOnlyDictionary<uint, SatellitePositionDto> Satellites => _satellites;

    public async Task EnsureStartedAsync(CancellationToken cancellationToken = default)
    {
        await _stream.StartAsync(cancellationToken);
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
