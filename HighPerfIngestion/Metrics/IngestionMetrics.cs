namespace HighPerfIngestion.Metrics;

public sealed class IngestionMetrics
{
    public long ProducedTotal;
    public long ConsumedTotal;

    public long TotalProcessingTicks;

    private long _minProcessingTicks = long.MaxValue;
    private long _maxProcessingTicks = 0;

    public long MinProcessingTicks => Volatile.Read(ref _minProcessingTicks);
    public long MaxProcessingTicks => Volatile.Read(ref _maxProcessingTicks);
    
    public void RecordProcessing(long elapsedTicks)
    {
        Interlocked.Increment(ref ConsumedTotal);
        Interlocked.Add(ref TotalProcessingTicks, elapsedTicks);

        UpdateMin(elapsedTicks);
        UpdateMax(elapsedTicks);
    }
    private void UpdateMin(long value)
    {
        long current;
        while ((current = Volatile.Read(ref _minProcessingTicks)) > value &&
               Interlocked.CompareExchange(ref _minProcessingTicks, value, current) != current)
        {
        }
    }

    private void UpdateMax(long value)
    {
        long current;
        while ((current = Volatile.Read(ref _maxProcessingTicks)) < value &&
               Interlocked.CompareExchange(ref _maxProcessingTicks, value, current) != current)
        {
        }
    }
}