using OrderService.Domain.Common;
using OrderService.Domain.Entities;
using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Aggregates;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _orderItems = new();

    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order() 
    {
        ShippingAddress = null!;
        TotalAmount = null!;
        Status = null!;
    }

    private Order(Guid customerId, Address shippingAddress) : base()
    {
        if (customerId == Guid.Empty)
            throw new OrderDomainException("Customer ID cannot be empty");

        if (shippingAddress == null)
            throw new OrderDomainException("Shipping address is required");

        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        TotalAmount = new Money(0, "USD");
    }

    public static Order Create(Guid customerId, Address shippingAddress, List<(Guid ProductId, string ProductName, Money UnitPrice, int Quantity)> items)
    {
        if (items == null || items.Count == 0)
            throw new OrderDomainException("Order must have at least one item");

        var order = new Order(customerId, shippingAddress);

        foreach (var item in items)
        {
            var orderItem = new OrderItem(order.Id, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
            order._orderItems.Add(orderItem);
        }

        order.RecalculateTotal();
        order.AddDomainEvent(new OrderPlacedDomainEvent(order.Id, order.CustomerId, order.TotalAmount.Amount));

        return order;
    }

    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new OrderDomainException($"Cannot add items to an order in {Status} status");

        var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(Id, productId, productName, unitPrice, quantity);
            _orderItems.Add(orderItem);
        }

        RecalculateTotal();
        AddDomainEvent(new OrderItemAddedDomainEvent(Id, productId, quantity));
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new OrderDomainException($"Cannot remove items from an order in {Status} status");

        var item = _orderItems.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            throw new OrderDomainException($"Product {productId} not found in order");

        _orderItems.Remove(item);

        if (_orderItems.Count == 0)
            throw new OrderDomainException("Order must have at least one item");

        RecalculateTotal();
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new OrderDomainException($"Cannot confirm an order in {Status} status. Only Pending orders can be confirmed.");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedDomainEvent(Id));
    }

    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Confirmed)
            throw new OrderDomainException($"Cannot ship an order in {Status} status. Only Confirmed orders can be shipped.");

        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new OrderDomainException("Tracking number is required");

        Status = OrderStatus.Shipped;
        AddDomainEvent(new OrderShippedDomainEvent(Id, trackingNumber));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new OrderDomainException($"Cannot cancel an order in {Status} status");

        if (Status == OrderStatus.Cancelled)
            throw new OrderDomainException("Order is already cancelled");

        if (string.IsNullOrWhiteSpace(reason))
            throw new OrderDomainException("Cancellation reason is required");

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new OrderDomainException($"Cannot deliver an order in {Status} status. Only Shipped orders can be delivered.");

        Status = OrderStatus.Delivered;
    }

    private void RecalculateTotal()
    {
        if (_orderItems.Count == 0)
        {
            TotalAmount = new Money(0, "USD");
            return;
        }

        var firstItem = _orderItems[0];
        var total = new Money(firstItem.TotalPrice.Amount, firstItem.TotalPrice.Currency);
        
        for (int i = 1; i < _orderItems.Count; i++)
        {
            total = total + _orderItems[i].TotalPrice;
        }

        TotalAmount = total;
    }
}
