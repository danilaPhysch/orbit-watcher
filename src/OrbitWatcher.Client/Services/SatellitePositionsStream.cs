using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using OrbitWatcher.Contracts;

namespace OrbitWatcher.Client.Services;

public sealed class SatellitePositionsStream(IOptions<SatelliteSignalRSettings> options) : IAsyncDisposable
{
    private readonly SatelliteSignalRSettings _settings = options.Value;
    private HubConnection? _connection;

    public event Action<IReadOnlyCollection<SatellitePositionDto>>? PositionsReceived;
    public event Action? StatusChanged;

    public SatelliteConnectionStatus Status { get; private set; } = SatelliteConnectionStatus.Disconnected;
    public DateTime? LastUpdateUtc { get; private set; }
    public string? LastError { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
        {
            return;
        }

        var connection = new HubConnectionBuilder()
            .WithUrl(BuildHubUrl())
            .WithAutomaticReconnect()
            .Build();

        _connection = connection;

        connection.On<IReadOnlyCollection<SatellitePositionDto>>(_settings.EventName, positions =>
        {
            LastUpdateUtc = DateTime.UtcNow;
            LastError = null;
            PositionsReceived?.Invoke(positions);
            NotifyStatusChanged();
        });

        connection.Reconnecting += error =>
        {
            LastError = error?.Message;
            SetStatus(SatelliteConnectionStatus.Reconnecting);
            return Task.CompletedTask;
        };

        connection.Reconnected += _ =>
        {
            LastError = null;
            SetStatus(SatelliteConnectionStatus.Connected);
            return Task.CompletedTask;
        };

        connection.Closed += error =>
        {
            LastError = error?.Message;
            SetStatus(error is null ? SatelliteConnectionStatus.Disconnected : SatelliteConnectionStatus.Error);
            return Task.CompletedTask;
        };

        try
        {
            SetStatus(SatelliteConnectionStatus.Connecting);
            await connection.StartAsync(cancellationToken);
            SetStatus(SatelliteConnectionStatus.Connected);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            SetStatus(SatelliteConnectionStatus.Error);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
        {
            return;
        }

        await _connection.DisposeAsync();
        _connection = null;
        SetStatus(SatelliteConnectionStatus.Disconnected);
    }

    private string BuildHubUrl()
    {
        if (!string.IsNullOrWhiteSpace(_settings.HubUrl))
        {
            return _settings.HubUrl;
        }

        var baseUri = new Uri(_settings.HubBaseUrl, UriKind.Absolute);
        var hubPath = _settings.HubPath.TrimStart('/');
        return new Uri(baseUri, hubPath).ToString();
    }

    private void SetStatus(SatelliteConnectionStatus status)
    {
        Status = status;
        NotifyStatusChanged();
    }

    private void NotifyStatusChanged() => StatusChanged?.Invoke();
}
