using CompanyName.Domain.Common;
using CompanyName.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    void MarkRowVersion<TEntity>(TEntity entity, byte[] rowVersion)
        where TEntity : class, IAuditable;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}