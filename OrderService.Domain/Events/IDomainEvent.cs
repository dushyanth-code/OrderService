using MediatR;

namespace OrderService.Domain.Events;
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
