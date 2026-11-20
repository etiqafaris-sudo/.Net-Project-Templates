using Mediator;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Uam.CreateStaff;

public sealed record StaffCreatedEvent(int UserId, string Email, string FullName) : INotification;

internal sealed class StaffCreatedEventHandler(ILogger<StaffCreatedEventHandler> logger)
    : INotificationHandler<StaffCreatedEvent>
{
    public ValueTask Handle(StaffCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Staff created: UserId={UserId}, Email={Email}, FullName={FullName}",
            notification.UserId,
            notification.Email,
            notification.FullName);
        return ValueTask.CompletedTask;
    }
}