using Microsoft.EntityFrameworkCore;
using SO17.Application.Abstractions;
using SO17.Application.Common;
using SO17.Domain.Entities;
namespace SO17.Infrastructure.Persistence;

public sealed class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task<Page<Order>> ListAsync(PageQuery query, OrderStatus? status, CancellationToken ct)
    {
        var number = int.TryParse(query.Term.TrimStart('#', '№', ' '), out var parsed) ? parsed : 0;
        var source = db.Orders.AsNoTracking().Where(x =>
            (status == null || x.Status == status) &&
            (query.Term == "" || x.Id == number || x.Number.Contains(query.Term) || x.CustomerPhone.Contains(query.Term) || x.CustomerName.Contains(query.Term)));
        var total = await source.CountAsync(ct);
        var data = await source.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip(query.Skip).Take(query.PageSize).Include(x => x.Lines).AsSplitQuery().ToListAsync(ct);
        return new(data, total, query.Page, query.PageSize);
    }
    public Task<Order?> FindAsync(int id, CancellationToken ct) => db.Orders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<(int Customers, int Items, int ActiveOrders, decimal Revenue)> StatisticsAsync(CancellationToken ct)
    {
        var customers = await db.Customers.CountAsync(ct);
        var items = await db.Items.CountAsync(ct);
        var active = await db.Orders.CountAsync(x => x.Status == OrderStatus.Pending || x.Status == OrderStatus.Processing, ct);
        var revenue = await db.Orders.Where(x => x.Status == OrderStatus.Completed)
            .SelectMany(x => x.Lines).SumAsync(x => (decimal?)(x.Quantity * x.UnitPrice), ct) ?? 0;
        return (customers, items, active, revenue);
    }
    public void Add(Order order) => db.Orders.Add(order);
}
