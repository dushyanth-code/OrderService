using OrderService.Domain.Common;

namespace OrderService.Domain.Entities;

public class OutboxMessage : Entity
{
    public string EventType { get; private set; }
    public Guid AggregateId { get; private set; }
    public string EventData { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsProcessed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    private OutboxMessage()
    {
        EventType = string.Empty;
        EventData = string.Empty;
    }

    public OutboxMessage(string eventType, Guid aggregateId, string eventData, DateTime occurredOn)
    {
        EventType = eventType;
        AggregateId = aggregateId;
        EventData = eventData;
        OccurredOn = occurredOn;
        CreatedAt = DateTime.UtcNow;
        IsProcessed = false;
        RetryCount = 0;
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = errorMessage;
    }
}
