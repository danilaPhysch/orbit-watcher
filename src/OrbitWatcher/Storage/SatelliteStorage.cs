using System.Collections.Immutable;
using SGPdotNET.Observation;

namespace OrbitWatcher.Storage;

public sealed class SatelliteStorage
{
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

    private sealed class SatelliteSnapshot(ImmutableDictionary<uint, Satellite> satellitesByNorad)
    {
        public static SatelliteSnapshot Empty { get; } = new(ImmutableDictionary<uint, Satellite>.Empty);

        public ImmutableDictionary<uint, Satellite> SatellitesByNorad { get; } = satellitesByNorad;

        public Satellite[] Satellites { get; } = [..satellitesByNorad.Values];

        public int Count => SatellitesByNorad.Count;
    }
}
