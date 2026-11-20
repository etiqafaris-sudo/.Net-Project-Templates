using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Shared;
using Mediator;

namespace CompanyName.Application.Features.Users.Commands.CreateNewUser
{
    public sealed record CreateNewUserCommand : IRequest<Result<int>>
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public string? IcNo { get; set; }
        public string? Role { get; set; }
    }

    internal sealed class CreateNewUserCommandHandler : IRequestHandler<CreateNewUserCommand, Result<int>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IEncryptionService _encryption;

        public CreateNewUserCommandHandler(ITemplateDbContext context, IEncryptionService encryption)
        {
            _context = context;
            _encryption = encryption;
        }
        public async ValueTask<Result<int>> Handle(CreateNewUserCommand request, CancellationToken cancellationToken)
        {
            var entity = new GlUser
            {
                Email = request.Email,
                Username = request.Username,
                Password = _encryption.Encrypt(request.Password!),
                Fullname = request.Fullname,
                //IcNo = request.IcNo,
                IsActive = 1,
                IsDeleted = 0,
                Role = request.Role
            };
            _context.GlUsers.AddAsync(entity);
            entity.SetAuditable(Domain.Enums.OpTypeEnum.Create);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(entity.Id);
        }
    }
}
