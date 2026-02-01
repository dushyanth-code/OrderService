using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Events;

namespace OrderService.Application.EventHandlers;

/// <summary>
/// Handler for OrderCancelledDomainEvent
/// </summary>
public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledDomainEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Order cancelled - OrderId: {OrderId}, Reason: {Reason}",
            notification.OrderId,
            notification.Reason);

        // Additional logic such as:
        // - Send cancellation email
        // - Process refund
        // - Update inventory
        // - etc.

        return Task.CompletedTask;
    }
}
