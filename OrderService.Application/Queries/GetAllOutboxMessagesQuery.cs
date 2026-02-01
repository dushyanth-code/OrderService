using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries;

public record GetAllOutboxMessagesQuery : IRequest<List<OutboxMessageDto>>
{
    public bool? IsProcessed { get; init; }
    public int? MaxResults { get; init; } = 100;
}
