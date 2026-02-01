using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.QueryHandlers;

public class GetOrderEventsQueryHandler : IRequestHandler<GetOrderEventsQuery, List<OutboxMessageDto>>
{
    private readonly OrderDbContext _context;

    public GetOrderEventsQueryHandler(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<List<OutboxMessageDto>> Handle(GetOrderEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.OutboxMessages
            .Where(e => e.AggregateId == request.OrderId)
            .OrderBy(e => e.OccurredOn)
            .Select(e => new OutboxMessageDto
            {
                Id = e.Id,
                EventType = e.EventType,
                AggregateId = e.AggregateId,
                EventData = e.EventData,
                OccurredOn = e.OccurredOn,
                CreatedAt = e.CreatedAt,
                IsProcessed = e.IsProcessed,
                ProcessedAt = e.ProcessedAt,
                RetryCount = e.RetryCount,
                ErrorMessage = e.ErrorMessage
            })
            .ToListAsync(cancellationToken);

        return events;
    }
}
