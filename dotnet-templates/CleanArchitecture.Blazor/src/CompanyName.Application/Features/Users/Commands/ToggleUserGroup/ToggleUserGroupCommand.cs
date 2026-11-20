using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Users.EventHandlers;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Commands.ToggleUserGroup
{
    public sealed record ToggleUserGroupCommand(int GroupId) : IRequest<Result<bool>>;

    internal sealed class ToggleUserGroupCommandHandler : IRequestHandler<ToggleUserGroupCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public ToggleUserGroupCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(ToggleUserGroupCommand request, CancellationToken cancellationToken)
        {
            var group = await _context.GlUserGroups.FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
            if (group is null)
            {
                return Result.Failure<bool>($"User group with ID {request.GroupId} not found.");
            }

            group.IsActive = group.IsActive == 0 ? 1 : 0;
            group.SetAuditable(Domain.Enums.OpTypeEnum.Update);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new UserGroupCreatedEvent(group), cancellationToken);
            return Result.Success(group.IsActive == 1);
        }
    }
}
