using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Commands;

/// <summary>
/// Command to create a new order
/// </summary>
public record CreateOrderCommand : IRequest<OrderDto>
{
    public Guid CustomerId { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public List<CreateOrderItemDto> Items { get; init; } = new();
}
