using System.Threading.Channels;
using HighPerfIngestion.Domain;
using HighPerfIngestion.Processing;
using HighPerfIngestion.Producers;
using HighPerfIngestion.Infrastructure;

Console.WriteLine("Starting Phase 6 — Thread Pool & Starvation Behavior Test...");

var cts = new CancellationTokenSource();

// ----------------------------
// CHANNEL CONFIG
// ----------------------------

bool useBounded = true;
int capacity = 2000;

var eventChannel = useBounded
    ? new EventChannel(capacity)
    : new EventChannel(null);

// ----------------------------
// PROCESSOR & CONSUMER
// ----------------------------

var processor = new EventProcessor();

var consumer = new EventConsumer(eventChannel.Reader, WorkloadType.Mixed, processor)
{
    EnableArtificialSlowness = true,
    ArtificialDelayMs = 3,
    EnableRandomFreeze = true
};

var consumerTask = Task.Run(() => consumer.StartAsync(cts.Token));

// ----------------------------
// PRODUCERS + WRITE HOOK
// ----------------------------

async ValueTask OnEventAsync(Event ev)
{
    try
    {
        await eventChannel.WriteAsync(ev, cts.Token);
    }
    catch (OperationCanceledException)
    {
        // shutting down
    }
}

var producers = new IEventProducer[]
{
    new FastProducer(),
    new BurstProducer(),
    new SlowBigPayloadProducer(),
    new ErraticProducer()
};

var producerTasks = producers
    .Select(p => Task.Run(() => p.RunAsync(OnEventAsync, cts.Token)))
    .ToArray();

// ----------------------------
// MONITOR TASK (Phase 6)
// ----------------------------

var monitorTask = Task.Run((Func<Task>)(async () =>
{
    while (!cts.Token.IsCancellationRequested)
    {
        ThreadPool.GetAvailableThreads(out int aw, out int ai);
        ThreadPool.GetMaxThreads(out int mw, out int mi);

        Console.WriteLine(
            $"[Monitor] ApproxChannelLength={eventChannel.ApproximateCount} | ThreadPool Workers {aw}/{mw}"
        );

        await Task.Delay(1000, cts.Token);
    }
}));

// ----------------------------
// BURST TEST (10,000 events)
// ----------------------------

int burstEvents = 10_000;
Console.WriteLine($"Sending {burstEvents} events for burst test...");

for (int i = 0; i < burstEvents; i++)
{
    await OnEventAsync(new Event(
        Guid.NewGuid(),
        DateTime.UtcNow,
        "dummy-payload",
        "burst-test"));
}

Console.WriteLine("Burst events sent.");

// ----------------------------
// RUNTIME WINDOW
// ----------------------------

Console.WriteLine("Producers + consumer running (Phase 6). Press ENTER to stop...");
Console.ReadLine();

cts.Cancel();

// ----------------------------
// SHUTDOWN
// ----------------------------

try
{
    await Task.WhenAll(producerTasks);
    await consumerTask;
    await monitorTask;
}
catch (OperationCanceledException)
{
    // normal shutdown
}

Console.WriteLine("Phase 6 complete.");
