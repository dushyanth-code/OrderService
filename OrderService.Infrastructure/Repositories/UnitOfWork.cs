using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Domain.Common;
using OrderService.Domain.Entities;
using OrderService.Domain.Events;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;
using System.Text.Json;

namespace OrderService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;
    private readonly IMediator _mediator;
    private IDbContextTransaction? _transaction;
    private IOrderRepository? _orderRepository;

    public UnitOfWork(OrderDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await StoreEventsInOutboxAsync(cancellationToken);
        
        var result = await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    private async Task StoreEventsInOutboxAsync(CancellationToken cancellationToken)
    {
        var domainEntities = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage(
                domainEvent.GetType().Name,
                GetAggregateIdFromEvent(domainEvent),
                JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions { WriteIndented = false }),
                domainEvent.OccurredOn
            );

            await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        }

        domainEntities.ForEach(entity => entity.ClearDomainEvents());
    }

    private Guid GetAggregateIdFromEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrderPlacedDomainEvent e => e.OrderId,
            OrderConfirmedDomainEvent e => e.OrderId,
            OrderCancelledDomainEvent e => e.OrderId,
            OrderShippedDomainEvent e => e.OrderId,
            OrderItemAddedDomainEvent e => e.OrderId,
            _ => Guid.Empty
        };
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
