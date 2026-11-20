using CompanyName.Application.Common.Interfaces;
using CompanyName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CompanyName.Infrastructure.Services;

public class Dal : IDal
{
    private readonly TemplateDbContext _dbContext;
    private IDbContextTransaction? _currentTransaction;

    public Dal(TemplateDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransAsync()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task<object?> ReadDataAsync(Type blType, bool isNeedReadAudit, object[] keys)
    {
        return await _dbContext.FindAsync(blType, keys);
    }

    public async Task<object?> CreateDataAsync(Type blType, object bl)
    {
        await _dbContext.AddAsync(bl);
        // Note: SaveChanges is called in CommitAsync usually, but for ID generation we might need it.
        // However, the pattern suggests explicit Commit.
        return bl;
    }

    public async Task UpdateDataAsync(Type blType, object bl, object? oldBl = null)
    {
        _dbContext.Update(bl);
        await Task.CompletedTask;
    }

    public async Task DeleteDataAsync(Type blType, object bl)
    {
        _dbContext.Remove(bl);
        await Task.CompletedTask;
    }

    public async Task<T?> GetDataByExpressionAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
    {
        return await _dbContext.Set<T>().FirstOrDefaultAsync(predicate);
    }

    public async Task<System.Collections.Generic.IEnumerable<T>> GetListByExpressionAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
    {
        return await _dbContext.Set<T>().Where(predicate).ToListAsync();
    }
}
