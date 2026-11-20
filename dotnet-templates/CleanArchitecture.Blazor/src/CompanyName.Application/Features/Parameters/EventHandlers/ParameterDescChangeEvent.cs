using Mediator;
using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Features.Parameters.EventHandlers
{
    public sealed record ParameterDescChangeEvent : INotification;

    internal sealed class ParameterDescChangeEventHandler : INotificationHandler<ParameterDescChangeEvent>
    {
        private readonly IMemoryCache _memoryCache;

        public ParameterDescChangeEventHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public ValueTask Handle(ParameterDescChangeEvent notification, CancellationToken cancellationToken)
        {
            // Handle the event here
            _memoryCache.Remove("LookupParameterService_GetAllParametersAsync");
            return ValueTask.CompletedTask;
        }
    }
}
