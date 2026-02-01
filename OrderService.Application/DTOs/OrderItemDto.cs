namespace OrderService.Application.DTOs;

/// <summary>
/// Data transfer object for order item information
/// </summary>
public record OrderItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal TotalPrice { get; init; }
}
