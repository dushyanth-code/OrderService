using MediatR;

namespace OrderService.Application.Commands;

/// <summary>
/// Command to ship an order
/// </summary>
public record ShipOrderCommand : IRequest<Unit>
{
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}
