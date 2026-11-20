using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Users.EventHandlers;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Commands.DeleteUserGroup
{
    public sealed record DeleteUserGroupCommand : IRequest<Result<bool>>
    {
        public int GroupId { get; set; }
    }

    internal sealed class DeleteUserGroupCommandHandler : IRequestHandler<DeleteUserGroupCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public DeleteUserGroupCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(DeleteUserGroupCommand request, CancellationToken cancellationToken)
        {
            var group = await _context.GlUserGroups.FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);
            if (group is null)
            {
                return Result<bool>.Failure("User group not found.");
            }

            group.IsDeleted = 1;
            group.SetAuditable(Domain.Enums.OpTypeEnum.Delete);
            _context.GlUserGroups.Update(group);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new UserGroupCreatedEvent(group), cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
