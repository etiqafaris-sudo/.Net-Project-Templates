using CompanyName.Domain.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Uam.UpdateUserStatus;

internal sealed class UpdatedUserStatusEventHandler(ILogger<UpdatedUserStatusEventHandler> logger)
    : INotificationHandler<UpdatedUserStatusEvent>
{
    public ValueTask Handle(UpdatedUserStatusEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User status updated: UserId={UserId}",
            notification.UserId);
        return ValueTask.CompletedTask;
    }
}