namespace OrderService.Domain.Events;
public record OrderItemAddedDomainEvent : IDomainEvent
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public DateTime OccurredOn { get; init; }

    public OrderItemAddedDomainEvent(Guid orderId, Guid productId, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        OccurredOn = DateTime.UtcNow;
    }
}
