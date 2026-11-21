namespace CompanyName.Domain.Common;

public abstract class AuditableEntity : Entity, IAuditable
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}