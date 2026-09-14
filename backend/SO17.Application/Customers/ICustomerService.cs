using SO17.Application.Common;
namespace SO17.Application.Customers;

public interface ICustomerService
{
    Task<Page<CustomerDto>> ListAsync(PageQuery query, CancellationToken ct);
    Task<CustomerDto> GetAsync(int id, CancellationToken ct);
    Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct);
    Task<CustomerDto> UpdateAsync(int id, CustomerRequest request, CancellationToken ct);
    Task DeleteAsync(int id, Guid version, CancellationToken ct);
}
