using System.Threading.Channels;
using HighPerfIngestion.Domain;
using HighPerfIngestion.Processing;
using HighPerfIngestion.Producers;
using HighPerfIngestion.Infrastructure;

Console.WriteLine("Starting Phase 5.3 Burst Test — Channel + Consumer + EventProcessor + 10k events...");

var cts = new CancellationTokenSource();

bool useBounded = true;
int capacity = 2000;

// Shared channel
var eventChannel = useBounded
    ? new EventChannel(capacity)
    : new EventChannel(null);

// Create processor
var processor = new EventProcessor();

// Consumer that uses the processor
var consumer = new EventConsumer(eventChannel.Reader, WorkloadType.Mixed, processor);
var consumerTask = Task.Run(() => consumer.StartAsync(cts.Token));

// This is what producers call — pushing into channel
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

// Create producers
var producers = new IEventProducer[]
{
    new FastProducer(),
    new BurstProducer(),
    new SlowBigPayloadProducer(),
    new ErraticProducer()
};

// Start producers
var producerTasks = producers
    .Select(p => Task.Run(() => p.RunAsync(OnEventAsync, cts.Token)))
    .ToArray();

// --- Burst 10k events ---
int burstEvents = 10_000;
Console.WriteLine($"Sending {burstEvents} events for burst test...");
for (int i = 0; i < burstEvents; i++)
{
    await OnEventAsync(new Event(
        Guid.NewGuid(),
        DateTime.UtcNow,
        "dummy-payload",
        "burst-test"
    ));
}
Console.WriteLine("Burst events sent.");

// Keep producers/consumer running for a while to observe metrics
Console.WriteLine("Producers + consumer running. Press ENTER to stop...");
Console.ReadLine();

cts.Cancel();

// Shutdown sequence
try
{
    await Task.WhenAll(producerTasks);
    await consumerTask;
}
catch (OperationCanceledException)
{
    // Normal shutdown
}

Console.WriteLine("Burst test done.");
