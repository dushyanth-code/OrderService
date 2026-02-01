using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Commands;

/// <summary>
/// Handler for ShipOrderCommand
/// </summary>
public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShipOrderCommandHandler> _logger;

    public ShipOrderCommandHandler(IUnitOfWork unitOfWork, ILogger<ShipOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shipping order {OrderId}", request.OrderId);

        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
            throw new OrderDomainException($"Order {request.OrderId} not found");

        order.Ship(request.TrackingNumber);
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} shipped successfully with tracking number {TrackingNumber}", 
            request.OrderId, request.TrackingNumber);

        return Unit.Value;
    }
}
