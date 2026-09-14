using System.ComponentModel.DataAnnotations;
using SO17.Domain.Entities;

namespace SO17.Application.Orders;

public sealed class OrderLineRequest
{
    [Range(1, int.MaxValue)] public int ItemId { get; init; }
    [Range(1, 1000000)] public int Quantity { get; init; }
}
public sealed class CreateOrderRequest
{
    [Range(1, int.MaxValue)] public int CustomerId { get; init; }
    [Required, MinLength(1), MaxLength(100)] public List<OrderLineRequest> Lines { get; init; } = [];
}
public sealed class StatusRequest
{
    [EnumDataType(typeof(OrderStatus))] public OrderStatus Status { get; init; }
    public Guid Version { get; init; }
}
public sealed record OrderLineDto(int ItemId, string Title, string Sku, int Quantity, decimal UnitPrice, decimal Total);
public sealed record OrderDto(int Id, string Number, DateTimeOffset CreatedAt, int CustomerId, string CustomerName,
    string CustomerPhone, OrderStatus Status, decimal Total, IReadOnlyList<OrderLineDto> Lines, Guid Version);
public sealed record DashboardDto(int Customers, int Items, int ActiveOrders, decimal Revenue, IReadOnlyList<OrderDto> RecentOrders);
