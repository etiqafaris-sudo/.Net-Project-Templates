using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Commands.AssignGroupUser
{
    public sealed record AssignGroupUserCommand(int GroupId, IReadOnlyList<int> SelectedUserIds) : IRequest<Result<bool>>;

    internal sealed class AssignGroupUserCommandHandler : IRequestHandler<AssignGroupUserCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;

        public AssignGroupUserCommandHandler(ITemplateDbContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<bool>> Handle(AssignGroupUserCommand request, CancellationToken cancellationToken)
        {
            var groupExists = await _context.GlUserGroups.AsNoTracking()
                .AnyAsync(g => g.Id == request.GroupId && g.IsDeleted == 0, cancellationToken);

            if (!groupExists)
            {
                return Result<bool>.Failure($"User group with id {request.GroupId} was not found.");
            }

            var groupIdStr = request.GroupId.ToString();
            var selectedIds = request.SelectedUserIds.Distinct().ToHashSet();

            var currentlyAssigned = await _context.GlUsers
                .Where(u => u.IsDeleted == 0 && u.Role == groupIdStr)
                .ToListAsync(cancellationToken);

            foreach (var user in currentlyAssigned.Where(u => !selectedIds.Contains(u.Id)))
            {
                user.Role = null;
                user.SetAuditable(Domain.Enums.OpTypeEnum.Update);
            }

            if (selectedIds.Count > 0)
            {
                var usersToAssign = await _context.GlUsers
                    .Where(u => u.IsDeleted == 0 && selectedIds.Contains(u.Id) && u.Role != groupIdStr)
                    .ToListAsync(cancellationToken);

                foreach (var user in usersToAssign)
                {
                    user.Role = groupIdStr;
                    user.SetAuditable(Domain.Enums.OpTypeEnum.Update);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
