using CompanyName.Domain.Enums;

namespace CompanyName.Domain.Common;

public interface IAuditable
{
    public int? IsActive { get; set; }
    public int? IsDeleted { get; set; }
    DateTime? AudAddDate { get; set; }
    int? AudAddBy { get; set; }
    DateTime? AudModDate { get; set; }
    int? AudModBy { get; set; }
    string? AudAction { get; set; }
    DateTime? AudDate { get; set; }
    
    bool IsNeedAudit { get; set; }
    void SetAudDetail(object userProfile, AuditEventTypeEnum auditEventType);
}
