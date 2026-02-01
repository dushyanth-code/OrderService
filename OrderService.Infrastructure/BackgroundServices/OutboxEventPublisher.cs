using MediatR;
using OrderService.Domain.Events;

namespace OrderService.Infrastructure.BackgroundServices;

public interface IOutboxEventPublisher
{
    Task PublishAsync(string eventType, string eventData, CancellationToken cancellationToken = default);
}

public class OutboxEventPublisher : IOutboxEventPublisher
{
    private readonly IMediator _mediator;

    public OutboxEventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAsync(string eventType, string eventData, CancellationToken cancellationToken = default)
    {
        var domainEvent = DeserializeEvent(eventType, eventData);
        
        if (domainEvent != null)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    private IDomainEvent? DeserializeEvent(string eventType, string eventData)
    {
        return eventType switch
        {
            nameof(OrderPlacedDomainEvent) => System.Text.Json.JsonSerializer.Deserialize<OrderPlacedDomainEvent>(eventData),
            nameof(OrderConfirmedDomainEvent) => System.Text.Json.JsonSerializer.Deserialize<OrderConfirmedDomainEvent>(eventData),
            nameof(OrderCancelledDomainEvent) => System.Text.Json.JsonSerializer.Deserialize<OrderCancelledDomainEvent>(eventData),
            nameof(OrderShippedDomainEvent) => System.Text.Json.JsonSerializer.Deserialize<OrderShippedDomainEvent>(eventData),
            nameof(OrderItemAddedDomainEvent) => System.Text.Json.JsonSerializer.Deserialize<OrderItemAddedDomainEvent>(eventData),
            _ => null
        };
    }
}
