using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Events;

namespace OrderService.Application.EventHandlers;

/// <summary>
/// Handler for OrderShippedDomainEvent
/// </summary>
public class OrderShippedEventHandler : INotificationHandler<OrderShippedDomainEvent>
{
    private readonly ILogger<OrderShippedEventHandler> _logger;

    public OrderShippedEventHandler(ILogger<OrderShippedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Order shipped - OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
            notification.OrderId,
            notification.TrackingNumber);

        // Additional logic such as:
        // - Send shipping notification email
        // - Update tracking system
        // - Notify customer service
        // - etc.

        return Task.CompletedTask;
    }
}
