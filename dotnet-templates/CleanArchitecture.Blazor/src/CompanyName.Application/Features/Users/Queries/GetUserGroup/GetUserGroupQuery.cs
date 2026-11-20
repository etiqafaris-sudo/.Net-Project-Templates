using CompanyName.Application.Common.Constants;
using CompanyName.Application.Common.Extensions;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Features.Users.Queries.GetUserGroup
{
    public sealed record GetUserGroupQuery : IRequest<Result<IReadOnlyList<GetUserGroupResponse>>>;

    internal sealed class GetUserGroupQueryHandler : IRequestHandler<GetUserGroupQuery, Result<IReadOnlyList<GetUserGroupResponse>>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public GetUserGroupQueryHandler(ITemplateDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public async ValueTask<Result<IReadOnlyList<GetUserGroupResponse>>> Handle(GetUserGroupQuery request, CancellationToken cancellationToken)
        {
            var result = await _memoryCache.GetOrCreateExclusiveAsync<IReadOnlyList<GetUserGroupResponse>>("GetUserGroupQuery", async entry =>
            {
                entry.AbsoluteExpiration = DateTime.Now.AddHours(8);
                return await _context.GlUserGroups.AsNoTracking()
                    .Where(ug => ug.IsDeleted == 0)
                    .Select(m => new GetUserGroupResponse(m.Id, m.GroupCode, m.GroupName, m.IsActive.Value))
                    .ToListAsync(cancellationToken);
            }, CacheEntry.BaseEntryOptions);

            return Result<IReadOnlyList<GetUserGroupResponse>>.Success(result);
        }
    }
}
