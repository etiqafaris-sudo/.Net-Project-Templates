using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;
using CompanyName.Domain.Events;

namespace CompanyName.Domain.Entities;

public sealed class User : AuditableEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    public static User Create(string email, string fullName)
    {
        var user = new User
        {
            Email = email,
            FullName = fullName,
            Status = UserStatus.Active,
        };

        user.RaiseDomainEvent(new StaffCreatedEvent(email, fullName));
        return user;
    }

    public void ChangeStatus(UserStatus status)
    {
        if (Status == status)
        {
            return;
        }

        Status = status;
        RaiseDomainEvent(new UpdatedUserStatusEvent(Id));
    }
}