using SGPdotNET.Observation;

namespace OrbitWatcher.Storage;

public interface ISatelliteStorage
{
    int Count { get; }

    bool TryGetByNoradCatId(uint noradCatId, out Satellite? satellite);

    IReadOnlyCollection<Satellite> GetAllSnapshot();

    void ReplaceAll(IEnumerable<KeyValuePair<uint, Satellite>> satellitesByNoradCatId);
}
