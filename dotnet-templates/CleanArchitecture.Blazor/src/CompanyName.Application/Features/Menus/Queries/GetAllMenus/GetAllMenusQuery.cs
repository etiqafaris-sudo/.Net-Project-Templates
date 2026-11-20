using CompanyName.Application.Common.Constants;
using CompanyName.Application.Common.Extensions;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CompanyName.Application.Features.Menus.Queries.GetAllMenus
{
    public sealed record GetAllMenusQuery() : IRequest<Result<IReadOnlyList<GetAllMenusReponse>>>;

    internal sealed class GetAllMenusQueryHandler : IRequestHandler<GetAllMenusQuery, Result<IReadOnlyList<GetAllMenusReponse>>>
    {
        private readonly ITemplateDbContext _applicationDbContext;
        private readonly IMemoryCache _cacheService;
        private readonly ILogger<GetAllMenusQueryHandler> _logger;

        public GetAllMenusQueryHandler(ITemplateDbContext applicationDbContext, IMemoryCache cacheService,ILogger<GetAllMenusQueryHandler> logger)
        {
            _applicationDbContext = applicationDbContext;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async ValueTask<Result<IReadOnlyList<GetAllMenusReponse>>> Handle(GetAllMenusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var menus = await _cacheService.GetOrCreateExclusiveAsync<IReadOnlyList<GetAllMenusReponse>>(nameof(GetAllMenusQuery), async entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromHours(8));
                    return await _applicationDbContext.GlPortalMenus.AsNoTracking()
                        .Where(x => x.IsDeleted == 0)
                        .Select(x => new GetAllMenusReponse
                        {
                            Id = x.Id,
                            MenuName = x.MenuName,
                            MenuUrl = x.MenuUrl,
                            Icon = x.Icon,
                            ParentId = x.ParentId,
                            OrderNo = x.OrderNo,
                            IsActive = x.IsActive
                        })
                        .ToListAsync(cancellationToken);
                }, CacheEntry.BaseEntryOptions);

                if (menus is null)
                    return Result.Success<IReadOnlyList<GetAllMenusReponse>>(Array.Empty<GetAllMenusReponse>()); //faris juz return emty list instead of null

                return Result.Success(menus);
            }
            catch (Exception ex)
            {
                return Result.Failure<IReadOnlyList<GetAllMenusReponse>>(ex.Message);
            }

        }
    }
}
