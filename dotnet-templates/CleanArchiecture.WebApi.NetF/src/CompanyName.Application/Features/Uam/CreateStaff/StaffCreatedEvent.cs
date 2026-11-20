using Mediator;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Uam.CreateStaff;

public sealed record StaffCreatedEvent(int UserId, string Email, string FullName) : INotification;

internal sealed class StaffCreatedEventHandler : INotificationHandler<StaffCreatedEvent>
{
    private readonly ILogger<StaffCreatedEventHandler> _logger;

    public StaffCreatedEventHandler(ILogger<StaffCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(StaffCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Staff created: UserId={UserId}, Email={Email}, FullName={FullName}",
            notification.UserId,
            notification.Email,
            notification.FullName);
        return default;
    }
}