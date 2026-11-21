using CompanyName.Domain.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Uam.CreateStaff;

internal sealed class StaffCreatedEventHandler(ILogger<StaffCreatedEventHandler> logger)
    : INotificationHandler<StaffCreatedEvent>
{
    public ValueTask Handle(StaffCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Staff created: Email={Email}, FullName={FullName}",
            notification.Email,
            notification.FullName);
        return ValueTask.CompletedTask;
    }
}