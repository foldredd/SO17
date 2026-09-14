using SO17.Application.Abstractions;
using SO17.Application.Common;
using SO17.Domain.Entities;

namespace SO17.Application.Items;

public sealed class ItemService(IItemRepository items, IUnitOfWork unitOfWork) : IItemService
{
    public async Task<Page<ItemDto>> ListAsync(PageQuery query, CancellationToken ct)
    {
        Validation.Ensure(query);
        var page = await items.ListAsync(query, ct);
        return new(page.Items.Select(Map).ToList(), page.Total, page.PageNumber, page.PageSize);
    }

    public async Task<ItemDto> GetAsync(int id, CancellationToken ct) => Map(await FindAsync(id, ct));

    public async Task<ItemDto> CreateAsync(ItemRequest request, CancellationToken ct)
    {
        Validate(request);
        var sku = request.Sku.Trim().ToUpperInvariant();
        await CheckSkuAsync(sku, 0, ct);
        var item = new Item { Title = request.Title.Trim(), Sku = sku, Quantity = request.Quantity, Price = request.Price };
        items.Add(item);
        await unitOfWork.SaveAsync(ct);
        return Map(item);
    }

    public async Task<ItemDto> UpdateAsync(int id, ItemRequest request, CancellationToken ct)
    {
        Validate(request);
        var item = await FindAsync(id, ct);
        Validation.Version(item.Version, request.Version);
        var sku = request.Sku.Trim().ToUpperInvariant();
        await CheckSkuAsync(sku, id, ct);
        item.Title = request.Title.Trim();
        item.Sku = sku;
        item.Quantity = request.Quantity;
        item.Price = request.Price;
        await unitOfWork.SaveAsync(ct);
        return Map(item);
    }

    public async Task DeleteAsync(int id, Guid version, CancellationToken ct)
    {
        var item = await FindAsync(id, ct);
        Validation.Version(item.Version, version);
        if (await items.HasOrdersAsync(id, ct))
            throw AppException.Conflict("Нельзя удалить товар, который используется в заказах.");
        items.Remove(item);
        await unitOfWork.SaveAsync(ct);
    }

    private static void Validate(ItemRequest request)
    {
        Validation.Ensure(request);
        if (decimal.Round(request.Price, 2) != request.Price)
            throw new AppException(400, "Цена должна содержать не более двух знаков после запятой.");
    }

    private async Task<Item> FindAsync(int id, CancellationToken ct) =>
        await items.FindAsync(id, ct) ?? throw AppException.NotFound();

    private async Task CheckSkuAsync(string sku, int exceptId, CancellationToken ct)
    {
        if (await items.SkuExistsAsync(sku, exceptId, ct))
            throw AppException.Conflict("Товар с таким артикулом уже существует.");
    }

    private static ItemDto Map(Item item) => new(item.Id, item.Title, item.Sku, item.Quantity, item.Price, item.Version);
}
