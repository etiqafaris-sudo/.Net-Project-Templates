using System;
using System.Threading.Tasks;

namespace CompanyName.Application.Common.Interfaces;

public interface IDal
{
    // Transaction Management
    Task BeginTransAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task SaveChangesAsync(); // Expose this so EF Core hits the DB to generate Identity values mid-transaction

    // CRUD
    Task<object?> ReadDataAsync(Type blType, bool isNeedReadAudit, object[] keys);
    Task<object?> CreateDataAsync(Type blType, object bl);
    Task UpdateDataAsync(Type blType, object bl, object? oldBl = null);
    Task DeleteDataAsync(Type blType, object bl);
    Task<T?> GetDataByExpressionAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;
    Task<System.Collections.Generic.IEnumerable<T>> GetListByExpressionAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;
}
