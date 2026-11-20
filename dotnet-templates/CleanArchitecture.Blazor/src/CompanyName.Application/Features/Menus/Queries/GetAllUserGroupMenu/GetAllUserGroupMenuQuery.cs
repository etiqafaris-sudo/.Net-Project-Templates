using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Menus.Queries.GetAllUserGroupMenu;

public sealed record GetAllUserGroupMenuQuery(int GroupId) : IRequest<Result<IReadOnlyList<UserGroupMenuAssignmentDto>>>;

public sealed record UserGroupMenuAssignmentDto(int Id, int? GroupId, int? MenuId);

internal sealed class GetAllUserGroupMenuQueryHandler : IRequestHandler<GetAllUserGroupMenuQuery, Result<IReadOnlyList<UserGroupMenuAssignmentDto>>>
{
    private readonly ITemplateDbContext _context;

    public GetAllUserGroupMenuQueryHandler(ITemplateDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<IReadOnlyList<UserGroupMenuAssignmentDto>>> Handle(
        GetAllUserGroupMenuQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _context.GlUserGroupMenus.AsNoTracking()
            .Where(x => x.GroupId == request.GroupId && x.IsActive == 1 && x.IsDeleted == 0)
            .Select(x => new UserGroupMenuAssignmentDto(x.Id, x.GroupId, x.MenuId))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<UserGroupMenuAssignmentDto>>.Success(result);
    }
}
