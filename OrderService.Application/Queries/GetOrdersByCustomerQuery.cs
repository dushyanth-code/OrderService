using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries;

/// <summary>
/// Query to get all orders for a customer
/// </summary>
public record GetOrdersByCustomerQuery : IRequest<List<OrderDto>>
{
    public Guid CustomerId { get; init; }
}
