using Mediator;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Uam.UpdateUserStatus;

public sealed record UpdatedUserStatusEvent(int userId) : INotification;

internal sealed class UpdatedUserStatusEventHandler(ILogger<UpdatedUserStatusEventHandler> logger) : INotificationHandler<UpdatedUserStatusEvent>
{
    public ValueTask Handle(UpdatedUserStatusEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User status updated: UserId={userId}",
            notification.userId);
        // Handle the event, e.g., log it or perform additional actions
        return ValueTask.CompletedTask;
    }
}
