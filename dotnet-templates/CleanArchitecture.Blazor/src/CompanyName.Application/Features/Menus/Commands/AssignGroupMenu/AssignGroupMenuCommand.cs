using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.Application.Features.Menus.Commands.AssignGroupMenu;

public sealed record AssignGroupMenuCommand(int GroupId, IReadOnlyList<int> SelectedMenuIds) : IRequest<Result<bool>>;

internal sealed class AssignGroupMenuCommandHandler : IRequestHandler<AssignGroupMenuCommand, Result<bool>>
{
    private readonly ITemplateDbContext _context;
    private readonly IMemoryCache _cache;

    public AssignGroupMenuCommandHandler(ITemplateDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async ValueTask<Result<bool>> Handle(AssignGroupMenuCommand request, CancellationToken cancellationToken)
    {
        var groupExists = await _context.GlUserGroups.AsNoTracking()
            .AnyAsync(g => g.Id == request.GroupId && g.IsDeleted == 0, cancellationToken);

        if (!groupExists)
        {
            return Result<bool>.Failure($"User group with id {request.GroupId} was not found.");
        }

        var selectedIds = request.SelectedMenuIds.Distinct().ToHashSet();

        var existingAssignments = await _context.GlUserGroupMenus
            .Where(x => x.GroupId == request.GroupId && x.IsDeleted == 0)
            .ToListAsync(cancellationToken);

        foreach (var assignment in existingAssignments.Where(a => a.MenuId.HasValue && !selectedIds.Contains(a.MenuId.Value)))
        {
            assignment.IsDeleted = 1;
            assignment.SetAuditable(OpTypeEnum.Update);
        }

        var existingMenuIds = existingAssignments
            .Where(a => a.MenuId.HasValue)
            .Select(a => a.MenuId!.Value)
            .ToHashSet();

        foreach (var menuId in selectedIds.Where(id => !existingMenuIds.Contains(id)))
        {
            var assignment = new GlUserGroupMenu
            {
                GroupId = request.GroupId,
                MenuId = menuId,
                IsActive = 1,
                IsDeleted = 0
            };
            assignment.SetAuditable(OpTypeEnum.Create);
            _context.GlUserGroupMenus.Add(assignment);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _cache.Remove($"GetAllMenuByUserQuery_{request.GroupId}");
        return Result<bool>.Success(true);
    }
}
