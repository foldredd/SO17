using SO17.Application.Common;
using SO17.Domain.Entities;

namespace SO17.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Page<Customer>> ListAsync(PageQuery query, CancellationToken ct);
    Task<Customer?> FindAsync(int id, CancellationToken ct);
    Task<bool> PhoneExistsAsync(string phone, int exceptId, CancellationToken ct);
    Task<bool> HasOrdersAsync(int id, CancellationToken ct);
    void Add(Customer customer);
    void Remove(Customer customer);
}
public interface IItemRepository
{
    Task<Page<Item>> ListAsync(PageQuery query, CancellationToken ct);
    Task<Item?> FindAsync(int id, CancellationToken ct);
    Task<List<Item>> FindManyAsync(IEnumerable<int> ids, CancellationToken ct);
    Task<bool> SkuExistsAsync(string sku, int exceptId, CancellationToken ct);
    Task<bool> HasOrdersAsync(int id, CancellationToken ct);
    void Add(Item item);
    void Remove(Item item);
}
public interface IOrderRepository
{
    Task<Page<Order>> ListAsync(PageQuery query, OrderStatus? status, CancellationToken ct);
    Task<Order?> FindAsync(int id, CancellationToken ct);
    Task<(int Customers, int Items, int ActiveOrders, decimal Revenue)> StatisticsAsync(CancellationToken ct);
    void Add(Order order);
}
public interface IUnitOfWork
{
    Task SaveAsync(CancellationToken ct);
}
