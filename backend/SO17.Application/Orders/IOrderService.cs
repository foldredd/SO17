using SO17.Application.Common;
using SO17.Domain.Entities;
namespace SO17.Application.Orders;

public interface IOrderService
{
    Task<Page<OrderDto>> ListAsync(PageQuery query, OrderStatus? status, CancellationToken ct);
    Task<OrderDto> GetAsync(int id, CancellationToken ct);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, string userId, CancellationToken ct);
    Task<OrderDto> ChangeStatusAsync(int id, StatusRequest request, CancellationToken ct);
    Task<DashboardDto> DashboardAsync(CancellationToken ct);
}
