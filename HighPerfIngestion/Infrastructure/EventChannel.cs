using System.Threading.Channels;
using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Infrastructure;

public class EventChannel
{
    private readonly Channel<Event> _channel;

    // Phase 6: approximate backlog counter (write-only)
    private int _count;
    public int ApproximateCount => Volatile.Read(ref _count);

    public ChannelWriter<Event> Writer => _channel.Writer;
    public ChannelReader<Event> Reader => _channel.Reader;

    public EventChannel(int? capacity)
    {
        if (capacity.HasValue)
        {
            var options = new BoundedChannelOptions(capacity.Value)
            {
                SingleWriter = false,
                SingleReader = false,
                FullMode = BoundedChannelFullMode.Wait // producers block when full
            };

            _channel = Channel.CreateBounded<Event>(options);
        }
        else
        {
            _channel = Channel.CreateUnbounded<Event>();
        }
    }

    /// <summary>
    /// Writes an event to the channel and increments the approximate count.
    /// </summary>
    public async ValueTask WriteAsync(Event ev, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(ev, ct);
        Interlocked.Increment(ref _count);
    }

    /// <summary>
    /// TryWrite variant — increments count only on success.
    /// </summary>
    public bool TryWrite(Event ev)
    {
        if (_channel.Writer.TryWrite(ev))
        {
            Interlocked.Increment(ref _count);
            return true;
        }

        return false;
    }
}