using CompanyName.Application.Common.Interfaces;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Auth.Commands.Logout
{
    public sealed record UserLogoutEvent(int UserId) : INotification;

    internal sealed class UserLogoutEventHandler : INotificationHandler<UserLogoutEvent>
    {
        private readonly ILogger<UserLogoutEventHandler> _logger;
        private readonly IMemoryCache _cache;
        private readonly ITemplateDbContext _applicationDbContext;
        public UserLogoutEventHandler(ILogger<UserLogoutEventHandler> logger, IMemoryCache cache, ITemplateDbContext applicationDbContext)
        {
            _logger = logger;
            _cache = cache;
            _applicationDbContext = applicationDbContext;
        }

        public async ValueTask Handle(UserLogoutEvent notification, CancellationToken cancellationToken)
        {
            var user = await _applicationDbContext.GlUsers.AsNoTracking()
                                                          .FirstOrDefaultAsync(u => u.Id == notification.UserId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found during logout.", notification.UserId);
                return;
            }

            _logger.LogWarning("User with ID {UserId} has logged out.", notification.UserId);
            // Clear any user-specific cache if needed
            _cache.Remove($"GetAllMenuByUserQuery_{user.Role}");
            _logger.LogWarning("User with ID {UserId} menu cache clear.", notification.UserId);
        }
    }


}
