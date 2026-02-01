namespace OrderService.Domain.Events;
public record OrderConfirmedDomainEvent : IDomainEvent
{
    public Guid OrderId { get; init; }
    public DateTime OccurredOn { get; init; }

    public OrderConfirmedDomainEvent(Guid orderId)
    {
        OrderId = orderId;
        OccurredOn = DateTime.UtcNow;
    }
}
