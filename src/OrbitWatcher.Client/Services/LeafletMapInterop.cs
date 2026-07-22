using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OrbitWatcher.Contracts;

namespace OrbitWatcher.Client.Services;

public sealed class LeafletMapInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private IJSObjectReference? _module;

    public async ValueTask InitializeAsync(string mapId, ElementReference mapElement, double centerLat, double centerLon, int zoom)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("initializeMap", mapId, mapElement, centerLat, centerLon, zoom);
    }

    public async ValueTask UpsertMarkersAsync(string mapId, IReadOnlyCollection<SatellitePositionDto> positions)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("upsertMarkers", mapId, positions);
    }

    public async ValueTask RemoveMarkersExceptAsync(string mapId, IReadOnlyCollection<uint> noradIds)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("removeMarkersExcept", mapId, noradIds);
    }

    public async ValueTask DisposeMapAsync(string mapId)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("disposeMap", mapId);
    }

    public async ValueTask DrawGroundTrackAsync(string mapId, GroundTrackDto groundTrack)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("drawGroundTrack", mapId, groundTrack.Segments);
    }

    public async ValueTask ClearGroundTrackAsync(string mapId)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("clearGroundTrack", mapId);
    }

    public async ValueTask SetMarkerClickCallbackAsync<T>(string mapId, DotNetObjectReference<T> dotNetRef, string methodName) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("setMarkerClickCallback", mapId, dotNetRef, methodName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        await _module.DisposeAsync();
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/leafletMapInterop.js");
        return _module;
    }
}
