using CompanyName.Application.Common.Constants;
using CompanyName.Application.Common.Extensions;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;
using CompanyName.Application.Features.Menus.Queries.GetAllMenus;
using CompanyName.Shared;
using Dapper;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Features.Menus.Queries.GetAllMenuByUser
{
    public sealed record GetAllMenuByUserQuery(int userGroupId) : IRequest<Result<IReadOnlyList<GetAllMenuByUserResponse>>>;

    internal sealed class GetAllMenuByUserQueryHandler : IRequestHandler<GetAllMenuByUserQuery, Result<IReadOnlyList<GetAllMenuByUserResponse>>>
    {
        private readonly ITemplateDbContext _applicationDbContext;
        private readonly IMemoryCache _memCache;
        private readonly IMediator _mediator;

        public GetAllMenuByUserQueryHandler(
            ITemplateDbContext applicationDbContext,
            IMemoryCache memCache,
            IMediator mediator)
        {
            _applicationDbContext = applicationDbContext;
            _memCache = memCache;
            _mediator = mediator;
        }

        public async ValueTask<Result<IReadOnlyList<GetAllMenuByUserResponse>>> Handle(GetAllMenuByUserQuery request, CancellationToken cancellationToken)
        {
            var allMenusResult = await _mediator.Send(new GetAllMenusQuery(), cancellationToken);
            var allActiveMenus = allMenusResult.IsSuccess ? allMenusResult.Value : null;

            if (allActiveMenus is null || allActiveMenus.Count == 0)
                return Result.Success<IReadOnlyList<GetAllMenuByUserResponse>>(Array.Empty<GetAllMenuByUserResponse>());

            string cacheKey = $"GetAllMenuByUserQuery_{request.userGroupId}";
            var userMenus = await _memCache.GetOrCreateExclusiveAsync<IReadOnlyList<GetAllMenuByUserResponseDto>>(cacheKey, async entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(8));
                return await _applicationDbContext.GlUserGroupMenus.AsNoTracking()
                .Where(x => (x.IsActive == 1 && x.IsDeleted == 0) && x.GroupId == request.userGroupId)
                .Select(x => new GetAllMenuByUserResponseDto
                {
                    Id = x.Id,
                    GroupId = x.GroupId,
                    MenuId = x.MenuId
                })
                .ToListAsync(cancellationToken);
            }, CacheEntry.BaseEntryOptions);

            if (userMenus is null)
                return Result.Success<IReadOnlyList<GetAllMenuByUserResponse>>(Array.Empty<GetAllMenuByUserResponse>());

            var allowedMenuIds = userMenus.Select(m => m.MenuId).ToHashSet();
            var elligibleMenus = allActiveMenus.Where(m => allowedMenuIds.Contains(m.Id)).AsList();

            if (elligibleMenus is null)
                return Result.Success<IReadOnlyList<GetAllMenuByUserResponse>>(Array.Empty<GetAllMenuByUserResponse>());

            var results = new List<GetAllMenuByUserResponse>();

            foreach (var menu in elligibleMenus)
            {
                results.Add(new GetAllMenuByUserResponse
                {
                    Icon = menu.Icon,
                    Id = menu.Id,
                    MenuName = menu.MenuName,
                    MenuUrl = menu.MenuUrl,
                    ParentId = menu.ParentId,
                    OrderNo = menu.OrderNo
                });
            }

            return Result.Success<IReadOnlyList<GetAllMenuByUserResponse>>(results);
        }
    }

    internal sealed record GetAllMenuByUserResponseDto
    {
        public int Id { get; set; }
        public int? GroupId { get; set; }
        public int? MenuId { get; set; }
    }
}
