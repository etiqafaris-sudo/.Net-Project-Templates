using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Users.EventHandlers;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Commands.UpdateUserGroup
{
    public sealed record UpdateUserGroupCommand(int id, string groupCode, string groupName, int IsActive) : IRequest<Result<bool>>;

    internal sealed class UpdateUserGroupCommandHandler : IRequestHandler<UpdateUserGroupCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public UpdateUserGroupCommandHandler(ITemplateDbContext context,IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(UpdateUserGroupCommand request, CancellationToken cancellationToken)
        {
            var ug = await _context.GlUserGroups.FirstOrDefaultAsync(x => x.Id == request.id, cancellationToken);

            if (ug is null)
                return Result<bool>.Failure($"User with Group Id {request.id} not exists");

            ug.GroupCode = request.groupCode;
            ug.GroupName = request.groupName;
            ug.IsActive = request.IsActive;

            ug.SetAuditable(Domain.Enums.OpTypeEnum.Update);

            _context.GlUserGroups.Update(ug);

            await _context.SaveChangesAsync(cancellationToken);

            //patut guna UserGroupUpatedEvent tapi since behavior sama then reused.
            _mediator.Publish(new UserGroupCreatedEvent(ug));
            return Result<bool>.Success(true);
        }
    }
}
