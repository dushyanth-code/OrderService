namespace OrderService.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new order item
/// </summary>
public record CreateOrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public int Quantity { get; init; }
}
