using System.Data.Entity;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Common;
using CompanyName.Domain.Entities;
using CompanyName.Infrastructure.Persistence.Configurations;

namespace CompanyName.Infrastructure.Persistence;

public class TemplateDbContext : DbContext, IApplicationDbContext
{
    public TemplateDbContext(string connectionString)
        : base(connectionString)
    {
    }

    public virtual DbSet<User> Users { get; set; } = null!;

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    public void MarkRowVersion<TEntity>(TEntity entity, byte[] rowVersion)
        where TEntity : class, IAuditable
    {
        Entry(entity).Property(e => e.RowVersion).OriginalValue = rowVersion;
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Configurations.Add(new UserConfiguration());
        modelBuilder.Configurations.Add(new OutboxMessageConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.ModifiedAtUtc = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAtUtc = utcNow;
            }
        }
    }
}
