using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SO17.Application.Common;
using SO17.Application.Orders;
using SO17.Domain.Entities;
namespace SO17.Api.Controllers;

[ApiController, Route("api/orders")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpGet]
    public Task<Page<OrderDto>> List([FromQuery] PageQuery query, [FromQuery] OrderStatus? status, CancellationToken ct) =>
        orders.ListAsync(query, status, ct);
    [HttpGet("{id:int}")]
    public Task<OrderDto> Get(int id, CancellationToken ct) => orders.GetAsync(id, ct);
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var order = await orders.CreateAsync(request, User.FindFirstValue(ClaimTypes.NameIdentifier)!, ct);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }
    [HttpPatch("{id:int}/status")]
    public Task<OrderDto> Status(int id, StatusRequest request, CancellationToken ct) => orders.ChangeStatusAsync(id, request, ct);
    [HttpGet("/api/dashboard")]
    public Task<DashboardDto> Dashboard(CancellationToken ct) => orders.DashboardAsync(ct);
}
