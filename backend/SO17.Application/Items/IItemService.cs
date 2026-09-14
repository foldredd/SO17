using SO17.Application.Common;
namespace SO17.Application.Items;

public interface IItemService
{
    Task<Page<ItemDto>> ListAsync(PageQuery query, CancellationToken ct);
    Task<ItemDto> GetAsync(int id, CancellationToken ct);
    Task<ItemDto> CreateAsync(ItemRequest request, CancellationToken ct);
    Task<ItemDto> UpdateAsync(int id, ItemRequest request, CancellationToken ct);
    Task DeleteAsync(int id, Guid version, CancellationToken ct);
}
