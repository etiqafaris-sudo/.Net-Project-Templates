using CompanyName.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyName.Domain.Common
{
    public abstract class AuditableEntity : IAuditable
    {
        [Column("is_active")]
        public int? IsActive { get; set; }

        [Column("is_deleted")]
        public int? IsDeleted { get; set; }

        [Column("aud_add_date")]
        public DateTime? AudAddDate { get; set; }

        [Column("aud_add_by")]
        public int? AudAddBy { get; set; }

        [Column("aud_mod_date")]
        public DateTime? AudModDate { get; set; }

        [Column("aud_mod_by")]
        public int? AudModBy { get; set; }

        [Column("aud_action")]
        public string? AudAction { get; set; }

        [Column("aud_date")]
        public DateTime? AudDate { get; set; }

        [NotMapped]
        public bool IsNeedAudit { get; set; }

        public void SetAudDetail(object userProfile, AuditEventTypeEnum auditEventType)
        {
        }
    }
}
