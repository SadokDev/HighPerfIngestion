using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;

public sealed class ErraticProducer : ProducerBase
{
    private readonly Random _rnd = new();

    public ErraticProducer() : base("ErraticProducer") { }

    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // 30% chance: sleep (like a slow producer)
            if (_rnd.NextDouble() < 0.3)
            {
                await Task.Delay(_rnd.Next(100, 500), ct);
            }
            else
            {
                // 70% chance: flood with events
                int flood = _rnd.Next(20, 200);
                for (int i = 0; i < flood; i++)
                {
                    await onEventAsync(CreateEvent("erratic"));
                }
            }
        }
    }
}