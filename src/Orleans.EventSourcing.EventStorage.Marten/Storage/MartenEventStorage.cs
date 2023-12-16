using System.Diagnostics;
using Marten;
using Marten.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using Exception = System.Exception;

namespace Orleans.EventSourcing.EventStorage.Marten;

/// <summary>
/// Event storage provider that stores events using a MartenDB event stream.
/// </summary>
[DebuggerDisplay("Marten:{" + nameof(_name) + "}")]
public class MartenEventStorage : IEventStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _name;
    private readonly string _serviceId;
    private readonly MartenOptions _options;
    private readonly ILogger<MartenEventStorage> _logger;
    private DocumentStore? _db;

    public MartenEventStorage(
        string name,
        MartenOptions options,
        ILogger<MartenEventStorage> logger,
        IOptions<ClusterOptions> clusterOptions
    )
    {
        _name = name;
        _options = options;
        _logger = logger;
        _serviceId = clusterOptions.Value.ServiceId;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<EventRecord<TEvent>> ReadEventsFromStorage<TEvent>(
        GrainId grainId, 
        int version = 0, 
        int maxCount = 2147483647
    ) where TEvent : class
    {
        if (version < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Version cannot be less than 0");
        }

        if (maxCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max Count cannot be less than 0");
        }

        await using var session = _db!.LightweightSession();

        var streamId = grainId.ToString();
        var lastIndexLimit = version >= int.MaxValue - maxCount ? int.MaxValue : version + maxCount;
        var events = await session.Events.FetchStreamAsync(streamId, version: lastIndexLimit, fromVersion: version + 1);
        
        foreach (var @event in events)
        {
            yield return new EventRecord<TEvent>((TEvent)@event.Data, (int)@event.Version);
        }
    }

    /// <inheritdoc />
    public async Task<bool> AppendEventsToStorage<TEvent>(
        GrainId grainId, 
        IEnumerable<TEvent> events, 
        int expectedVersion
    ) where TEvent : class
    {
        if (expectedVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedVersion), "Expected version cannot be less than 0");
        }

        var streamId = grainId.ToString();
        var eventList = events.ToList();
        
        await using var session = _db!.LightweightSession();

        if (expectedVersion == 0)
        {
            session.Events.StartStream(streamId, eventList);
        }
        else
        {
            session.Events.Append(streamId, expectedVersion + eventList.Count, events: eventList);
        }

        try
        {
            await session.SaveChangesAsync();
        }
        catch (Exception e) when (e is ExistingStreamIdCollisionException or EventStreamUnexpectedMaxEventIdException)
        {
            return false;
        }

        return true;
    }

    public void Participate(ISiloLifecycle lifecycle)
    {
        var name = OptionFormattingUtilities.Name<MartenEventStorage>(_name);
        lifecycle.Subscribe(name, _options.InitStage, Init, Close);
    }

    private async Task Init(CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();

        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "MartenEventStorage {Name} is initializing: ServiceId={ServiceId}",
                    _name,
                    _serviceId
                );
            }

            _db = await _options.CreateDocumentStore(_options.StoreOptions);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                timer.Stop();
                _logger.LogDebug(
                    "Init: Name={Name} ServiceId={ServiceId}, initialized in {ElapsedMilliseconds} ms",
                    _name,
                    _serviceId,
                    timer.Elapsed.TotalMilliseconds.ToString("0.00")
                );
            }
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(
                ex,
                "Init: Name={Name} ServiceId={ServiceId}, errored in {ElapsedMilliseconds} ms.",
                _name,
                _serviceId,
                timer.Elapsed.TotalMilliseconds.ToString("0.00")
            );

            throw new MartenEventStorageException($"{ex.GetType()}: {ex.Message}");
        }
    }

    private async Task Close(CancellationToken cancellationToken)
    {
        if (_db is null) return;

        await _db.DisposeAsync();
    }
}