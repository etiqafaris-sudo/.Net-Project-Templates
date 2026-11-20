using CompanyName.Domain.Entities;
using Mediator;
using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Features.Users.EventHandlers
{
    public sealed record UserGroupCreatedEvent(GlUserGroup GlUserGroup) : INotification;

    internal sealed class UserGroupCreatedEventHandler : INotificationHandler<UserGroupCreatedEvent>
    {
        private readonly IMemoryCache _memoryCache;

        public UserGroupCreatedEventHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public ValueTask Handle(UserGroupCreatedEvent notification, CancellationToken cancellationToken)
        {
            _memoryCache.Remove("GetUserGroupQuery");
            return ValueTask.CompletedTask;
        }
    }
}
