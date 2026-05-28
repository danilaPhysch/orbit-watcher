using System.Collections.Concurrent;
using SGPdotNET.Observation;

namespace OrbitWatcher.Storage;

public class SatelliteStorage
{
    private readonly ConcurrentDictionary<string, Satellite> _satellites = new();

    public void AddOrUpdate(IList<Satellite> satellites)
    {
        foreach (var satellite in satellites)
            _satellites[satellite.Name] = satellite;
    }

    public IReadOnlyList<Satellite> GetAll() => _satellites.Values.ToList();

    public Satellite? GetByName(string name) =>
        _satellites.GetValueOrDefault(name);
}