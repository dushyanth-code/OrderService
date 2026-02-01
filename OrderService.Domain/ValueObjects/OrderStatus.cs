using OrderService.Domain.Common;

namespace OrderService.Domain.ValueObjects;

/// <summary>
/// Value object representing order status
/// </summary>
public class OrderStatus : ValueObject
{
    public string Value { get; private set; }

    public static OrderStatus Pending => new("Pending");
    public static OrderStatus Confirmed => new("Confirmed");
    public static OrderStatus Shipped => new("Shipped");
    public static OrderStatus Delivered => new("Delivered");
    public static OrderStatus Cancelled => new("Cancelled");

    private OrderStatus() { Value = string.Empty; }

    private OrderStatus(string value)
    {
        Value = value;
    }

    public static OrderStatus FromString(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "pending" => Pending,
            "confirmed" => Confirmed,
            "shipped" => Shipped,
            "delivered" => Delivered,
            "cancelled" => Cancelled,
            _ => throw new ArgumentException($"Invalid order status: {status}", nameof(status))
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
