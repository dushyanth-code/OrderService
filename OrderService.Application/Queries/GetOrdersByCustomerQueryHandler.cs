using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Queries;

/// <summary>
/// Handler for GetOrdersByCustomerQuery
/// </summary>
public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrdersByCustomerQueryHandler> _logger;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository, ILogger<GetOrdersByCustomerQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving orders for customer {CustomerId}", request.CustomerId);

        var orders = await _orderRepository.GetOrdersByCustomerIdAsync(request.CustomerId, cancellationToken);

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            ShippingAddress = new AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                ZipCode = order.ShippingAddress.ZipCode,
                Country = order.ShippingAddress.Country
            },
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            Status = order.Status.Value,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice = oi.UnitPrice.Amount,
                Currency = oi.UnitPrice.Currency,
                Quantity = oi.Quantity,
                TotalPrice = oi.TotalPrice.Amount
            }).ToList()
        }).ToList();
    }
}
