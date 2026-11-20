using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;

namespace CompanyName.Domain.Entities;

public sealed class User : AuditableEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
}