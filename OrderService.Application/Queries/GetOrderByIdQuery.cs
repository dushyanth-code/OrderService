using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries;

/// <summary>
/// Query to get an order by ID
/// </summary>
public record GetOrderByIdQuery : IRequest<OrderDto?>
{
    public Guid OrderId { get; init; }
}
