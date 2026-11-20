using CompanyName.Application.Features.Menus.Queries.GetAllMenus;
using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Menus.Events
{
    public sealed record MenuUpdateEvent : INotification
    {
    }

    internal sealed class MenuUpdateEventHandler : INotificationHandler<MenuUpdateEvent>
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MenuUpdateEventHandler> _logger;

        public MenuUpdateEventHandler(IMemoryCache cache, ILogger<MenuUpdateEventHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public ValueTask Handle(MenuUpdateEvent notification, CancellationToken cancellationToken)
        {
            _cache.Remove(nameof(GetAllMenusQuery));
            _logger.LogWarning("Menu updated, cache cleared.");
            // Handle the event here
            return ValueTask.CompletedTask;
        }
    }
}
