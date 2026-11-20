using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;

namespace CompanyName.Application.Common;

/// <summary>
/// Use before Dapper/ADO insert or update when going through <see cref="Interfaces.IDbConnectionFactory"/>.
/// </summary>
public static class AuditableCudExtensions
{
    public static void SetAuditable(this IAuditable entity, OpTypeEnum opType, int userId = 0)
    {
        entity.IsNeedAudit = true;
        AuditableCudAudit.Apply(entity, opType, userId);
    }
}
