using CompanyName.Domain.Common;

namespace CompanyName.Domain.Events;

public sealed record UpdatedUserStatusEvent(int UserId) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}