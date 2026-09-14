using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SO17.Application.Abstractions;
using SO17.Application.Common;
namespace SO17.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw AppException.Conflict("Дані змінилися в іншому запиті. Оновіть сторінку та повторіть дію.");
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw AppException.Conflict("Запис із таким унікальним значенням уже існує.");
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            throw AppException.Conflict("Операція порушує зв’язки або обмеження бази даних.");
        }
    }
}
