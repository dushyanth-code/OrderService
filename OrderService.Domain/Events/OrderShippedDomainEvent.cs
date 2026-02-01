namespace OrderService.Domain.Events;
public record OrderShippedDomainEvent : IDomainEvent
{
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; }
    public DateTime OccurredOn { get; init; }

    public OrderShippedDomainEvent(Guid orderId, string trackingNumber)
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        OccurredOn = DateTime.UtcNow;
    }
}
