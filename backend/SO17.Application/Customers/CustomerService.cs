using SO17.Application.Abstractions;
using SO17.Application.Common;
using SO17.Domain.Entities;

namespace SO17.Application.Customers;

public sealed class CustomerService(ICustomerRepository customers, IUnitOfWork unitOfWork) : ICustomerService
{
    public async Task<Page<CustomerDto>> ListAsync(PageQuery query, CancellationToken ct)
    {
        Validation.Ensure(query);
        var page = await customers.ListAsync(query, ct);
        return new(page.Items.Select(Map).ToList(), page.Total, page.PageNumber, page.PageSize);
    }

    public async Task<CustomerDto> GetAsync(int id, CancellationToken ct) => Map(await FindAsync(id, ct));

    public async Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct)
    {
        Validation.Ensure(request);
        var phone = Validation.Phone(request.Phone);
        await CheckPhoneAsync(phone, 0, ct);
        var customer = new Customer { Name = request.Name.Trim(), Surname = request.Surname.Trim(), Phone = phone };
        customers.Add(customer);
        await unitOfWork.SaveAsync(ct);
        return Map(customer);
    }

    public async Task<CustomerDto> UpdateAsync(int id, CustomerRequest request, CancellationToken ct)
    {
        Validation.Ensure(request);
        var customer = await FindAsync(id, ct);
        Validation.Version(customer.Version, request.Version);
        var phone = Validation.Phone(request.Phone);
        await CheckPhoneAsync(phone, id, ct);
        customer.Name = request.Name.Trim();
        customer.Surname = request.Surname.Trim();
        customer.Phone = phone;
        await unitOfWork.SaveAsync(ct);
        return Map(customer);
    }

    public async Task DeleteAsync(int id, Guid version, CancellationToken ct)
    {
        var customer = await FindAsync(id, ct);
        Validation.Version(customer.Version, version);
        if (await customers.HasOrdersAsync(id, ct))
            throw AppException.Conflict("Нельзя удалить клиента с заказами: это нарушит историю продаж.");
        customers.Remove(customer);
        await unitOfWork.SaveAsync(ct);
    }

    private async Task<Customer> FindAsync(int id, CancellationToken ct) =>
        await customers.FindAsync(id, ct) ?? throw AppException.NotFound();

    private async Task CheckPhoneAsync(string phone, int exceptId, CancellationToken ct)
    {
        if (await customers.PhoneExistsAsync(phone, exceptId, ct))
            throw AppException.Conflict("Клієнт із таким телефоном уже існує.");
    }

    private static CustomerDto Map(Customer customer) =>
        new(customer.Id, customer.Name, customer.Surname, customer.Phone, customer.Version);
}
