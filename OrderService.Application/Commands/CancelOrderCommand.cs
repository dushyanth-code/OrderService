using MediatR;

namespace OrderService.Application.Commands;

/// <summary>
/// Command to cancel an order
/// </summary>
public record CancelOrderCommand : IRequest<Unit>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
