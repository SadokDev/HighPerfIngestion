using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;

public sealed class FastProducer : ProducerBase
{
    public FastProducer() : base("FastProducer") { }
    
    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var ev = CreateEvent("fast");
                await onEventAsync(ev);

                await Task.Delay(1, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected shutdown
        }
    }
}