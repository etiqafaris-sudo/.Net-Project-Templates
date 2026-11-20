using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Commands.DeleteUser
{
    public sealed record DeleteUserCommand : IRequest<Result<bool>>
    {
        public int UserId { get; set; }
    }

    internal sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        public DeleteUserCommandHandler(ITemplateDbContext context)
        {
            _context = context;
        }
        public async ValueTask<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.GlUsers.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user is null)
            {
                return Result<bool>.Failure("User not found.");
            }
            user.IsDeleted = 1;
            user.SetAuditable(Domain.Enums.OpTypeEnum.Delete);
            _context.GlUsers.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
