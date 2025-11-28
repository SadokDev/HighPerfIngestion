using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;

public sealed class BurstProducer : ProducerBase
{
    private readonly Random _rnd = new();

    public BurstProducer() : base("BurstProducer") { }

    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Random burst size between 5 and 50
            int burst = _rnd.Next(5, 50);

            for (int i = 0; i < burst; i++)
            {
                var ev = CreateEvent("burst");
                await onEventAsync(ev);
            }

            // Then rest for a random period
            await Task.Delay(_rnd.Next(50, 200), ct);
        }
    }
}