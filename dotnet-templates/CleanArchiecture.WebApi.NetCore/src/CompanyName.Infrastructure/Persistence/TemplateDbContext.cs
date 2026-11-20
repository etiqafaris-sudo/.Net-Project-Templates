using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Common;
using CompanyName.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CompanyName.Infrastructure.Persistence;

public sealed class TemplateDbContext(DbContextOptions<TemplateDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public void MarkRowVersion<TEntity>(TEntity entity, byte[] rowVersion)
        where TEntity : class, IAuditable
    {
        Entry(entity).Property(e => e.RowVersion).OriginalValue = rowVersion;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}