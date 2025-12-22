using System.Diagnostics;
using System.Threading.Channels;
using HighPerfIngestion.Domain;
using HighPerfIngestion.Metrics;

namespace HighPerfIngestion.Processing;

public enum WorkloadType
{
    CpuHeavy,
    IoBound,
    Mixed
}

public class EventConsumer
{
    private readonly ChannelReader<Event> _reader;
    private readonly EventProcessor _processor;
    private readonly WorkloadType _workloadType;
    private readonly IngestionMetrics _metrics;

    // ---- Phase 6 knobs ----
    public bool EnableArtificialSlowness { get; set; } = true;
    public bool EnableRandomFreeze { get; set; } = true;
    public int ArtificialDelayMs { get; set; } = 3;

    private long _processedCount = 0;

    public EventConsumer(
        ChannelReader<Event> reader,
        WorkloadType workloadType,
        EventProcessor processor,
        IngestionMetrics metrics)
    {
        _reader = reader;
        _workloadType = workloadType;
        _processor = processor;
        _metrics = metrics;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[Consumer] Started.");

        await foreach (var evt in _reader.ReadAllAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Your normal per-event processing (Phase 5 fast-path/slow-path).
            await ProcessEventAsync(evt, cancellationToken);

            // ---- Phase 6: make consumer intentionally slower ----
            if (EnableArtificialSlowness)
            {
                // Small async delay to reduce throughput and create pressure
                await Task.Delay(ArtificialDelayMs, cancellationToken);

                // ~0.02% chance (1 in 5000) to simulate a GC pause or lock stall
                if (EnableRandomFreeze && Random.Shared.Next(0, 5000) == 0)
                {
                    Console.WriteLine("[Consumer] Simulating random freeze (250–600ms)...");
                    await Task.Delay(Random.Shared.Next(250, 600), cancellationToken);
                }
            }

            // ---- Phase 6: Diagnostics every 2000 events ----
            if (Interlocked.Increment(ref _processedCount) % 2000 == 0)
            {
                LogThreadPoolState();
            }
        }

        Console.WriteLine("[Consumer] Stopped.");
    }

    private async ValueTask ProcessEventAsync(Event evt, CancellationToken ct)
    {
        long start = Stopwatch.GetTimestamp();

        await _processor.ProcessAsync(evt, ct);

        long elapsed = Stopwatch.GetTimestamp() - start;

        _metrics.RecordProcessing(elapsed);
    }

    // ---- Phase 6 diagnostics ----
    private void LogThreadPoolState()
    {
        ThreadPool.GetAvailableThreads(out int availableWorkers, out int availableIO);
        ThreadPool.GetMaxThreads(out int maxWorkers, out int maxIO);
        ThreadPool.GetMinThreads(out int minWorkers, out int minIO);

        Console.WriteLine(
            $"[ThreadPool] Worker: {availableWorkers}/{maxWorkers} | Min={minWorkers} | IO: {availableIO}/{maxIO}"
        );
    }
}
