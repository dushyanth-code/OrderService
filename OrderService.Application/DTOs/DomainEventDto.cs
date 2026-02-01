namespace OrderService.Application.DTOs;

public record OutboxMessageDto
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public Guid AggregateId { get; init; }
    public string EventData { get; init; } = string.Empty;
    public DateTime OccurredOn { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsProcessed { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public int RetryCount { get; init; }
    public string? ErrorMessage { get; init; }
}
