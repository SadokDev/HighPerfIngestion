using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;

public sealed class BurstProducer : ProducerBase
{
    private readonly Random _rnd = new();

    public BurstProducer() : base("BurstProducer") { }

    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                int burst = _rnd.Next(5, 50);

                for (int i = 0; i < burst; i++)
                {
                    await onEventAsync(CreateEvent("burst"));
                }

                await Task.Delay(_rnd.Next(50, 200), ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected shutdown
        }
    }
}