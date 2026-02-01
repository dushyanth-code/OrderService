using OrderService.Domain.Aggregates;

namespace OrderService.Domain.Repositories;
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
