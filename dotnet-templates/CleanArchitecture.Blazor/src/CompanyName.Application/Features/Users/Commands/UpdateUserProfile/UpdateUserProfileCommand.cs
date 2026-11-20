using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Commands.UpdateUserProfile
{
    public sealed record UpdateUserProfileCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string? IcNo { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
    }

    internal sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;

        public UpdateUserProfileCommandHandler(ITemplateDbContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<bool>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.GlUsers.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (user is null)
            {
                return Result<bool>.Failure("User not found.");
            }

            user.Email = request.Email;
            user.Fullname = request.Fullname;
            user.Role = request.Role;
            user.SetAuditable(Domain.Enums.OpTypeEnum.Update);

            _context.GlUsers.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
