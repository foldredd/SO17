using Microsoft.AspNetCore.Mvc;
using SO17.Application.Common;
using SO17.Application.Customers;
namespace SO17.Api.Controllers;

[ApiController, Route("api/customers")]
public sealed class CustomersController(ICustomerService customers) : ControllerBase
{
    [HttpGet]
    public Task<Page<CustomerDto>> List([FromQuery] PageQuery query, CancellationToken ct) => customers.ListAsync(query, ct);
    [HttpGet("{id:int}")]
    public Task<CustomerDto> Get(int id, CancellationToken ct) => customers.GetAsync(id, ct);
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerRequest request, CancellationToken ct)
    {
        var customer = await customers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
    }
    [HttpPut("{id:int}")]
    public Task<CustomerDto> Update(int id, CustomerRequest request, CancellationToken ct) => customers.UpdateAsync(id, request, ct);
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] Guid version, CancellationToken ct)
    {
        await customers.DeleteAsync(id, version, ct);
        return NoContent();
    }
}
