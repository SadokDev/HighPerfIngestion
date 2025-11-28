using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Producers;
public interface IEventProducer
{
    /// <summary>
    /// start the producer loop
    /// The callback represents "where events will be pushed".
    /// <summary>
    Task RunAsync(Func<Event, ValueTask> onEventAsync, CancellationToken ct);
}