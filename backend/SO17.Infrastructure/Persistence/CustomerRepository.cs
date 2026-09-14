using Microsoft.EntityFrameworkCore;
using SO17.Application.Abstractions;
using SO17.Application.Common;
using SO17.Domain.Entities;
namespace SO17.Infrastructure.Persistence;

public sealed class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public async Task<Page<Customer>> ListAsync(PageQuery query, CancellationToken ct)
    {
        var source = db.Customers.AsNoTracking().Where(x =>
            query.Term == "" || x.Name.Contains(query.Term) || x.Surname.Contains(query.Term) || x.Phone.Contains(query.Term));
        var total = await source.CountAsync(ct);
        var data = await source.OrderByDescending(x => x.Id).Skip(query.Skip).Take(query.PageSize).ToListAsync(ct);
        return new(data, total, query.Page, query.PageSize);
    }
    public Task<Customer?> FindAsync(int id, CancellationToken ct) => db.Customers.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> PhoneExistsAsync(string phone, int exceptId, CancellationToken ct) =>
        db.Customers.AnyAsync(x => x.Phone == phone && x.Id != exceptId, ct);
    public Task<bool> HasOrdersAsync(int id, CancellationToken ct) => db.Orders.AnyAsync(x => x.CustomerId == id, ct);
    public void Add(Customer customer) => db.Customers.Add(customer);
    public void Remove(Customer customer) => db.Customers.Remove(customer);
}
