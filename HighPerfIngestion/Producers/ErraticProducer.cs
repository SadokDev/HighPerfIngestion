using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;

public sealed class ErraticProducer : ProducerBase
{
    private readonly Random _rnd = new();

    public ErraticProducer() : base("ErraticProducer") { }

    public override async Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (_rnd.NextDouble() < 0.3)
                {
                    await Task.Delay(_rnd.Next(100, 500), ct);
                }
                else
                {
                    int flood = _rnd.Next(20, 200);
                    for (int i = 0; i < flood; i++)
                    {
                        await onEventAsync(CreateEvent("erratic"));
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected shutdown
        }
    }
}