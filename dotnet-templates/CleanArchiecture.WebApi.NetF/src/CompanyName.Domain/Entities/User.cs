using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;

namespace CompanyName.Domain.Entities;

public sealed class User : AuditableEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
}