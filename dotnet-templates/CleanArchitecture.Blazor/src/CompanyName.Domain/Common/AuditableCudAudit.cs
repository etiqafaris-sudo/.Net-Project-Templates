using CompanyName.Domain.Enums;

namespace CompanyName.Domain.Common;

public static class AuditableCudAudit
{
    public static void Apply(IAuditable bl, OpTypeEnum opType, int userId = 0)
    {
        var now = DateTime.Now;
        bl.AudDate = now;

        if (opType == OpTypeEnum.Create)
        {
            bl.AudAddDate = now;
            bl.AudAddBy = userId;
            bl.AudAction = "CIPTA REKOD";
        }
        else if (opType == OpTypeEnum.Update)
        {
            bl.AudModDate = now;
            bl.AudModBy = userId;
            bl.AudAction = bl.IsDeleted == 1 ? "PADAM REKOD" : "PINDA REKOD";
        }
        else if (opType == OpTypeEnum.Delete)
        {
            bl.AudModDate = now;
            bl.AudModBy = userId;
            bl.AudAction = "PADAM REKOD";
        }

        var auditEventType = opType switch
        {
            OpTypeEnum.Create => AuditEventTypeEnum.Create,
            OpTypeEnum.Delete => AuditEventTypeEnum.Delete,
            _ => AuditEventTypeEnum.Update
        };

        bl.SetAudDetail(userId, auditEventType);
    }
}
