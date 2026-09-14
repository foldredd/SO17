using Microsoft.AspNetCore.Mvc;
using SO17.Application.Common;
using SO17.Application.Items;
namespace SO17.Api.Controllers;

[ApiController, Route("api/items")]
public sealed class ItemsController(IItemService items) : ControllerBase
{
    [HttpGet]
    public Task<Page<ItemDto>> List([FromQuery] PageQuery query, CancellationToken ct) => items.ListAsync(query, ct);
    [HttpGet("{id:int}")]
    public Task<ItemDto> Get(int id, CancellationToken ct) => items.GetAsync(id, ct);
    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(ItemRequest request, CancellationToken ct)
    {
        var item = await items.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }
    [HttpPut("{id:int}")]
    public Task<ItemDto> Update(int id, ItemRequest request, CancellationToken ct) => items.UpdateAsync(id, request, ct);
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] Guid version, CancellationToken ct)
    {
        await items.DeleteAsync(id, version, ct);
        return NoContent();
    }
}
