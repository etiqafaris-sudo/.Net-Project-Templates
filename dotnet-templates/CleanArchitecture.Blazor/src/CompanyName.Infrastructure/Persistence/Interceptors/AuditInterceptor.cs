using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CompanyName.Infrastructure.Persistence.Interceptors;

internal sealed class AuditInterceptor : SaveChangesInterceptor
{
    private const int DefaultUserId = 0;
    private readonly ILogger<AuditInterceptor> _logger;
    public AuditInterceptor(ILogger<AuditInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (!entry.Entity.IsNeedAudit)
            {
                continue;
            }

            var opType = entry.State switch
            {
                EntityState.Added => OpTypeEnum.Create,
                EntityState.Modified => OpTypeEnum.Update,
                EntityState.Deleted => OpTypeEnum.Delete,
                _ => (OpTypeEnum?)null
            };

            if (opType is null)
            {
                continue;
            }

            AuditableCudAudit.Apply(entry.Entity, opType.Value, DefaultUserId);
            entry.Entity.IsNeedAudit = true;
        }
    }
}
