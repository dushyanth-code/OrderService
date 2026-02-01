namespace OrderService.Domain.Events;
public record OrderCancelledDomainEvent : IDomainEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; }
    public DateTime OccurredOn { get; init; }

    public OrderCancelledDomainEvent(Guid orderId, string reason)
    {
        OrderId = orderId;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }
}
