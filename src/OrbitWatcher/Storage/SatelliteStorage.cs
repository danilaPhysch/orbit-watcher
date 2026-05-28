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
    private SatelliteSnapshot _snapshot = SatelliteSnapshot.Empty;

    public int Count => Volatile.Read(ref _snapshot).Count;

    public bool TryGetByNoradCatId(uint noradCatId, out Satellite satellite)
    {
        var snapshot = Volatile.Read(ref _snapshot);
        return snapshot.SatellitesByNorad.TryGetValue(noradCatId, out satellite!);
    }

    public IReadOnlyCollection<Satellite> GetAllSnapshot()
    {
        return Volatile.Read(ref _snapshot).Satellites;
    }

    public void ReplaceAll(IEnumerable<KeyValuePair<uint, Satellite>> satellitesByNoradCatId)
    {
        var builder = ImmutableDictionary.CreateBuilder<uint, Satellite>();
        foreach (var (noradCatId, satellite) in satellitesByNoradCatId)
        {
            builder[noradCatId] = satellite;
        }

        Volatile.Write(ref _snapshot, new SatelliteSnapshot(builder.ToImmutable()));
    }

    private sealed class SatelliteSnapshot
    {
        public static SatelliteSnapshot Empty { get; } = new(ImmutableDictionary<uint, Satellite>.Empty);

        public SatelliteSnapshot(ImmutableDictionary<uint, Satellite> satellitesByNorad)
        {
            SatellitesByNorad = satellitesByNorad;
            Satellites = [..satellitesByNorad.Values];
        }

        public ImmutableDictionary<uint, Satellite> SatellitesByNorad { get; }

        public Satellite[] Satellites { get; }

        public int Count => SatellitesByNorad.Count;
    }
}
