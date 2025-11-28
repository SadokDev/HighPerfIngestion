using HighPerfIngestion.Domain;
using System.Threading.Channels;

namespace HighPerfIngestion.Infrastructure;

public sealed class EventChannel
{
    private readonly Channel<Event> _channel;
    public ChannelReader<Event> Reader => _channel.Reader;
    public ChannelWriter<Event> Writer => _channel.Writer;

    public EventChannel(int? capacity = null)
    {
        _channel =
            capacity is null
                ? Channel.CreateUnbounded<Event>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                })
                : Channel.CreateBounded<Event>(new BoundedChannelOptions(capacity.Value)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait // natural backpressure
                });
    }

    public ValueTask WriteAsync(Event ev, CancellationToken ct) =>
        Writer.WriteAsync(ev, ct);
}