using HighPerfIngestion.Domain;
using System.Text;

namespace HighPerfIngestion.Producers;

public sealed class SlowBigPayloadProducer : ProducerBase
{
    public SlowBigPayloadProducer() : base("SlowBigPayloadProducer") { }

    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // generate a heavy payload (~ several KB)
            var bigPayload = new string('X', 5000);

            var ev = new Event(
                id: Guid.NewGuid(),
                timestamp: DateTime.UtcNow,
                payload: bigPayload,
                type: "big"
            );

            await onEventAsync(ev);

            // Slow producer: 100ms pause
            await Task.Delay(100, ct);
        }
    }
}