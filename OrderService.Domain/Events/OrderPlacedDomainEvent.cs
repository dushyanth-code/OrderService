namespace OrderService.Domain.Events;
public record OrderPlacedDomainEvent : IDomainEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime OccurredOn { get; init; }

    public OrderPlacedDomainEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        OccurredOn = DateTime.UtcNow;
    }
}
