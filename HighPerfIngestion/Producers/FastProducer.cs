using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;

public sealed class FastProducer : ProducerBase
{
    public FastProducer() : base("FastProducer") { }
    
    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var ev = CreateEvent("fast");
            await onEventAsync(ev);
            
            // small sleep to avoid infinite tight spin
            await Task.Delay(1, ct);
        }
    }
}