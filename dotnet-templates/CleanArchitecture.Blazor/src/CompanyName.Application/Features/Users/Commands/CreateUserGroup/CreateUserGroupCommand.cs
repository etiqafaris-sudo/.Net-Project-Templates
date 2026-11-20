using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Users.EventHandlers;
using CompanyName.Domain.Entities;
using CompanyName.Shared;
using Mediator;

namespace CompanyName.Application.Features.Users.Commands.CreateUserGroup
{
    public sealed record CreateUserGroupCommand(string groupCode, string groupName, int IsActive) : IRequest<Result<bool>>;

    internal sealed class CreateUserGroupCommandHandler : IRequestHandler<CreateUserGroupCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator mediator;

        public CreateUserGroupCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            this.mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(CreateUserGroupCommand request, CancellationToken cancellationToken)
        {
            var gluser = new GlUserGroup
            {
                GroupCode = request.groupCode,
                GroupName = request.groupName,
                IsActive = 1,
                IsDeleted = 0
            };

            gluser.SetAuditable(Domain.Enums.OpTypeEnum.Create);
            await _context.GlUserGroups.AddAsync(gluser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await mediator.Publish(new UserGroupCreatedEvent(gluser));
            return Result<bool>.Success(true);
        }
    }
}
