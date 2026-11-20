using Mediator;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Uam.UpdateUserStatus;

public sealed record UpdatedUserStatusEvent(int UserId) : INotification;

internal sealed class UpdatedUserStatusEventHandler : INotificationHandler<UpdatedUserStatusEvent>
{
    private readonly ILogger<UpdatedUserStatusEventHandler> _logger;

    public UpdatedUserStatusEventHandler(ILogger<UpdatedUserStatusEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(UpdatedUserStatusEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User status updated: UserId={UserId}",
            notification.UserId);
        return default;
    }
}