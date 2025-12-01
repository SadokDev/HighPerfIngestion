using System.Threading.Channels;
using HighPerfIngestion.Domain;

namespace HighPerfIngestion.Processing;

public enum WorkloadType
{
    CpuHeavy,
    IoBound,
    Mixed
}
public class EventConsumer
{
    private readonly ChannelReader<Event>  _reader;
    private readonly WorkloadType _workloadType;
    public EventConsumer(ChannelReader<Event> reader, WorkloadType workloadType)
    {
        _reader = reader;
        _workloadType = workloadType;
    }

    /*Wait for data
        Read events one by one
    Stop when cancellation requested*/
    public async Task StartAsync(CancellationToken cancellationToken)
    {
       await foreach (var evt in _reader.ReadAllAsync(cancellationToken))
       {
           //_ = evt;
           await ProcessEventAsync(evt, cancellationToken);
       }
    }
    
    private async ValueTask ProcessEventAsync(Event evt, CancellationToken cancellationToken)
    {
        switch (_workloadType)
        {
            case WorkloadType.CpuHeavy:
                await SimulateCpuWorkAsync(evt, cancellationToken);
                break;

            case WorkloadType.IoBound:
                await SimulateIoWorkAsync(evt, cancellationToken);
                break;

            case WorkloadType.Mixed:
                await SimulateMixedWorkAsync(evt, cancellationToken);
                break;
        }
    }

    private async ValueTask SimulateMixedWorkAsync(Event evt, CancellationToken cancellationToken)
    {
        int roll = Random.Shared.Next(0, 100);
        // 70%: fast path (light CPU)
        if (roll < 70)
        {
            // small CPU burn
            Span<byte> input = stackalloc byte[16];
            evt.Id.TryWriteBytes(input);
            using var sha = System.Security.Cryptography.SHA256.Create(); 
            sha.TryComputeHash(input, stackalloc byte[32], out _);

            return;
        }
        
        // 20%: I/O-like async path
        if (roll < 90)
        {
            int simulatedLatencyMs = Random.Shared.Next(5, 20);
            await Task.Delay(simulatedLatencyMs, cancellationToken);
            return;
        }

        // 10%: heavy CPU path
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            Span<byte> input = stackalloc byte[16];
            evt.Id.TryWriteBytes(input);

            for (int i = 0; i < 300; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sha.TryComputeHash(input, stackalloc byte[32], out _);
            }
        }
    }

    private async ValueTask SimulateIoWorkAsync(Event evt, CancellationToken cancellationToken)
    {
        // We simulate variable I/O latency.
        // Typical real-world latency range: 5ms–25ms
        int simulatedLatencyMs = Random.Shared.Next(5, 25);
        await Task.Delay(simulatedLatencyMs, cancellationToken);
        // This is pure async I/O simulation.
        // No CPU burn, no thread blocking.
    }

    private ValueTask SimulateCpuWorkAsync(Event evt, CancellationToken cancellationToken)
    {
        // CPU-heavy simulation: compute a SHA256 hash repeatedly
        // This burns real CPU cycles and blocks the consumer thread.
        
        using var sha = System.Security.Cryptography.SHA256.Create();
        //Convert event ID to bytes( minimum allocation)

        Span<byte> input = stackalloc byte[16];
        evt.Id.TryWriteBytes(input);
        
        for (int i = 0; i < 500; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sha.TryComputeHash(input, stackalloc byte[32], out _);
        }
        
        // CPU work is synchronous, so return a completed ValueTask
        return ValueTask.CompletedTask;
    }
}