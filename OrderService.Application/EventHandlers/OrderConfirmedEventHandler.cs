using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Events;

namespace OrderService.Application.EventHandlers;

/// <summary>
/// Handler for OrderConfirmedDomainEvent
/// </summary>
public class OrderConfirmedEventHandler : INotificationHandler<OrderConfirmedDomainEvent>
{
    private readonly ILogger<OrderConfirmedEventHandler> _logger;

    public OrderConfirmedEventHandler(ILogger<OrderConfirmedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Order confirmed - OrderId: {OrderId}",
            notification.OrderId);

        // Additional logic such as:
        // - Send confirmation email
        // - Trigger payment processing
        // - Update warehouse system
        // - etc.

        return Task.CompletedTask;
    }
}
