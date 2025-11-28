using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;

public abstract class ProducerBase : IEventProducer
{
    protected readonly string Name;

    protected ProducerBase(string name)
    {
        Name = name;
    }
    public abstract Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct);
    protected static Event CreateEvent(string type = "default") => Event.Create(payload: "dummy", type: "default");
}