using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Events;

namespace OrderService.Application.EventHandlers;

/// <summary>
/// Handler for OrderPlacedDomainEvent
/// </summary>
public class OrderPlacedEventHandler : INotificationHandler<OrderPlacedDomainEvent>
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;

    public OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderPlacedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Order placed - OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}",
            notification.OrderId,
            notification.CustomerId,
            notification.TotalAmount);
        //Publish to Topic

        return Task.CompletedTask;
    }
}
