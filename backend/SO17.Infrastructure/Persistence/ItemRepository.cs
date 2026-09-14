using Microsoft.EntityFrameworkCore;
using SO17.Application.Abstractions;
using SO17.Application.Common;
using SO17.Domain.Entities;
namespace SO17.Infrastructure.Persistence;

public sealed class ItemRepository(AppDbContext db) : IItemRepository
{
    public async Task<Page<Item>> ListAsync(PageQuery query, CancellationToken ct)
    {
        var source = db.Items.AsNoTracking().Where(x =>
            query.Term == "" || x.Title.Contains(query.Term) || x.Sku.Contains(query.Term));
        var total = await source.CountAsync(ct);
        var data = await source.OrderByDescending(x => x.Id).Skip(query.Skip).Take(query.PageSize).ToListAsync(ct);
        return new(data, total, query.Page, query.PageSize);
    }
    public Task<Item?> FindAsync(int id, CancellationToken ct) => db.Items.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<List<Item>> FindManyAsync(IEnumerable<int> ids, CancellationToken ct) =>
        db.Items.Where(x => ids.Contains(x.Id)).ToListAsync(ct);
    public Task<bool> SkuExistsAsync(string sku, int exceptId, CancellationToken ct) =>
        db.Items.AnyAsync(x => x.Sku == sku && x.Id != exceptId, ct);
    public Task<bool> HasOrdersAsync(int id, CancellationToken ct) => db.OrderLines.AnyAsync(x => x.ItemId == id, ct);
    public void Add(Item item) => db.Items.Add(item);
    public void Remove(Item item) => db.Items.Remove(item);
}
