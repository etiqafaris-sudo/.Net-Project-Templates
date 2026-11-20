using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Commands.DeactivateUser
{
    public sealed record DeactivateUserCommand : IRequest<Result<bool>>
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }

    internal sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        public DeactivateUserCommandHandler(ITemplateDbContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<bool>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.GlUsers.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user is null)
            {
                return Result<bool>.Failure("User not found.");
            }

            user.IsActive = request.IsActive ? 1 : 0;
            user.SetAuditable(Domain.Enums.OpTypeEnum.Update);
            _context.GlUsers.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
