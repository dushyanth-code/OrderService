using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries;

public record GetOrderEventsQuery : IRequest<List<OutboxMessageDto>>
{
    public Guid OrderId { get; init; }
}
