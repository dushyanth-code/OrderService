namespace OrderService.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new order
/// </summary>
public record CreateOrderDto
{
    public Guid CustomerId { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public List<CreateOrderItemDto> Items { get; init; } = new();
}
