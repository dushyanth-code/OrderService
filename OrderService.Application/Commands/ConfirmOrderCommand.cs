using MediatR;

namespace OrderService.Application.Commands;

/// <summary>
/// Command to confirm an order
/// </summary>
public record ConfirmOrderCommand : IRequest<Unit>
{
    public Guid OrderId { get; init; }
}
