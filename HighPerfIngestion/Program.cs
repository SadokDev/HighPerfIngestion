using HighPerfIngestion.Domain;
using HighPerfIngestion.Producers;
using HighPerfIngestion.Infrastructure;
using HighPerfIngestion.Processing;   // <-- Needed for EventConsumer

Console.WriteLine("Starting Phase 3/4 — Channel + Consumer...");

var cts = new CancellationTokenSource();

bool useBounded = true;
int capacity = 2000;

// Create the shared channel
var eventChannel = useBounded
    ? new EventChannel(capacity)
    : new EventChannel(null);

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

// Pick your workload type here for Phase 4
var workloadType = WorkloadType.Mixed;   // <-- You can change: CpuHeavy / IoBound / Mixed

// Create the consumer
var consumer = new EventConsumer(eventChannel.Reader, workloadType);

// Start consumer on a background task
var consumerTask = Task.Run(() => consumer.StartAsync(cts.Token));

// Create producers
var producers = new IEventProducer[]
{
    new FastProducer(),
    new BurstProducer(),
    new SlowBigPayloadProducer(),
    new ErraticProducer()
};

// Start producers as background tasks
var producerTasks = producers
    .Select(p => Task.Run(() => p.RunAsync(OnEventAsync, cts.Token)))
    .ToArray();

Console.WriteLine("Producers + consumer started.");
Console.WriteLine("Press ENTER to stop...");

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

Console.WriteLine("Phase 4 done.");