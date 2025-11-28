using HighPerfIngestion.Domain;
using HighPerfIngestion.Producers;
using HighPerfIngestion.Infrastructure;

Console.WriteLine("Starting Phase 3 — Channel buffer...");

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

var producers = new IEventProducer[]
{
    new FastProducer(),
    new BurstProducer(),
    new SlowBigPayloadProducer(),
    new ErraticProducer()
};

// Start producers on background tasks
var tasks = producers
    .Select(p => Task.Run(() => p.RunAsync(OnEventAsync, cts.Token)))
    .ToArray();

Console.WriteLine("Producers started. Channel is filling.");
Console.WriteLine("Press ENTER to stop...");

Console.ReadLine();

cts.Cancel();
try
{
    await Task.WhenAll(tasks);
}
catch (OperationCanceledException)
{
    // Normal shutdown
}

Console.WriteLine("Phase 3 done.");