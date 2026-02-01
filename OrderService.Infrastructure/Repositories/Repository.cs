using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Common;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : AggregateRoot
{
    protected readonly OrderDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(OrderDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        var entry = _context.Entry(entity);
        
        if (entry.State == EntityState.Detached)
        {
            _dbSet.Update(entity);
        }
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
}
