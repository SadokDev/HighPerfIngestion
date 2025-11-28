// See https://aka.ms/new-console-template for more information

using HighPerfIngestion.Domain;
using HighPerfIngestion.Producers;

Console.WriteLine("Starting producers (Phase 2)...");
var cts = new CancellationTokenSource();

ValueTask FakeSink(Event ev)
{
    Console.WriteLine($"Produced: {ev.Type} at {ev.Timestamp:HH:mm:ss.fff}");
    return ValueTask.CompletedTask;
}

// Create Producers
var producers = new IEventProducer[]
{
    new FastProducer(),
    new BurstProducer(),
    new SlowBigPayloadProducer(),
    new ErraticProducer()
};

// Start producers
var tasks = producers
    .Select(p => Task.Run(() => p.RunAsync(FakeSink, cts.Token)))
    .ToArray();

Console.WriteLine("Press ENTER to stop...");
Console.ReadLine();
cts.Cancel();


await Task.WhenAll(tasks);




/*var ev = Event.Create("Hello","test");
Console.WriteLine($"Sample event created: {ev.Id} , {ev.Timestamp}, {ev.Payload}, {ev.Type}");*/