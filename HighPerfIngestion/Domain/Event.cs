namespace HighPerfIngestion.Domain;

public sealed class Event
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string Payload { get; init; } = string.Empty;
    public string Type { get; init; } = "default";
    
    public Event(Guid id, DateTime timestamp, string payload, string type){
        Id = id;
        Timestamp = timestamp;
        Payload = payload;
        Type = type;
    }
    public static Event Create(string? payload = null, string? type = null) =>
        new(id: Guid.NewGuid(), 
            timestamp:DateTime.UtcNow, 
            payload: payload ?? "dummy", 
            type: type?? "default"
            );
}