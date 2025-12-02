using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Processing;

public sealed record EventResult(Guid Id, string Data);

public class EventProcessor
{
    private readonly Dictionary<Guid, string> _cache = new();

    /// <summary>
    /// Processes one event.
    /// Fast-path is allocation-free. Slow-path wraps async Task in ValueTask.
    /// </summary>
    public ValueTask<EventResult> ProcessAsync(Event evt, CancellationToken ct)
    {
        int roll = Random.Shared.Next(0, 100);

        // 70% fast-path (synchronous, allocation-free)
        if (roll < 70)
        {
            return FastPath(evt);
        }

        // 30% slow-path (async) → wrap Task in ValueTask
        return new ValueTask<EventResult>(SlowPathAsync(evt, ct));
    }

    // ------------------------------
    // FAST-PATH (sync, zero allocation)
    // ------------------------------
    private ValueTask<EventResult> FastPath(Event evt)
    {
        if (_cache.TryGetValue(evt.Id, out string? cached))
        {
            return new ValueTask<EventResult>(new EventResult(evt.Id, cached));
        }

        string data = $"cached:{evt.Id.ToString()[..8]}";
        _cache[evt.Id] = data;

        return new ValueTask<EventResult>(new EventResult(evt.Id, data));
    }

    // ------------------------------
    // SLOW-PATH (async Task wrapped in ValueTask)
    // ------------------------------
    private async Task<EventResult> SlowPathAsync(Event evt, CancellationToken ct)
    {
        // Simulate variable I/O latency
        int latency = Random.Shared.Next(5, 20);
        await Task.Delay(latency, ct);

        string result = $"slow:{evt.Id.ToString()[..8]}";
        _cache[evt.Id] = result;

        return new EventResult(evt.Id, result);
    }
}