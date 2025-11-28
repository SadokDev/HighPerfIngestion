using HighPerfIngestion.Domain;
using System.Text;

namespace HighPerfIngestion.Producers;

public sealed class SlowBigPayloadProducer : ProducerBase
{
    public SlowBigPayloadProducer() : base("SlowBigPayloadProducer") { }

    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var bigPayload = new string('X', 5000);

                var ev = new Event(
                    id: Guid.NewGuid(),
                    timestamp: DateTime.UtcNow,
                    payload: bigPayload,
                    type: "big"
                );

                await onEventAsync(ev);

                await Task.Delay(100, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected shutdown
        }
    }
}