using System.Collections.Immutable;
using System.Threading;
using SGPdotNET.Observation;

namespace OrbitWatcher.Storage;

/// <summary>
/// In-memory snapshot storage optimized for frequent reads (e.g. per-second SignalR streaming)
/// and infrequent writes (e.g. daily ephemeris refresh).
/// </summary>
public sealed class SatelliteStorage
{
    // Atomic reference swap (rare writes). Readers take a snapshot reference (frequent reads).
    private ImmutableDictionary<int, Satellite> _satellitesByNorad = ImmutableDictionary<int, Satellite>.Empty;

    public int Count => Volatile.Read(ref _satellitesByNorad).Count;

    public bool TryGetByNoradCatId(int noradCatId, out Satellite satellite)
    {
        var snapshot = Volatile.Read(ref _satellitesByNorad);
        return snapshot.TryGetValue(noradCatId, out satellite!);
    }

    public IReadOnlyCollection<Satellite> GetAllSnapshot()
    {
        // Returns a stable snapshot view without additional allocations.
        var snapshot = Volatile.Read(ref _satellitesByNorad);
        return snapshot.Values;
    }

    public void ReplaceAll(IEnumerable<Satellite> satellites)
    {
        var builder = ImmutableDictionary.CreateBuilder<int, Satellite>();
        foreach (var satellite in satellites)
        {
            builder[satellite.OmmData.NoradCatId] = satellite;
        }

        Volatile.Write(ref _satellitesByNorad, builder.ToImmutable());
    }
}
