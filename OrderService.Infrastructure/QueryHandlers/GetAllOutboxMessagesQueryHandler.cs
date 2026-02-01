using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.QueryHandlers;

public class GetAllOutboxMessagesQueryHandler : IRequestHandler<GetAllOutboxMessagesQuery, List<OutboxMessageDto>>
{
    private readonly OrderDbContext _context;

    public GetAllOutboxMessagesQueryHandler(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<List<OutboxMessageDto>> Handle(GetAllOutboxMessagesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OutboxMessages.AsQueryable();

        if (request.IsProcessed.HasValue)
        {
            query = query.Where(e => e.IsProcessed == request.IsProcessed.Value);
        }

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Take(request.MaxResults ?? 100)
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
