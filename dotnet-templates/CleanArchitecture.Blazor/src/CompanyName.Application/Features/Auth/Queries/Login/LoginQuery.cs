using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Auth.Queries.Login
{
    public sealed record LoginQuery(string username, string password) : IRequest<Result<LoginQueryResponse>>;

    internal sealed class LoginQueryHandler : IRequestHandler<LoginQuery, Result<LoginQueryResponse>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IEncryptionService _encryption;

        public LoginQueryHandler(ITemplateDbContext context, IEncryptionService encryption)
        {
            _context = context;
            _encryption = encryption;
        }

        public async ValueTask<Result<LoginQueryResponse>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.GlUsers.AsNoTracking()
                .Where(x => x.Email == request.username && x.IsActive == 1 && x.IsDeleted == 0)
                .Select(u => new LoginQueryResponse
                {
                    Email = u.Email,
                    Fullname = u.Fullname,
                    Password = u.Password,
                    IcNo = u.IcNo,
                    Role = u.Role,
                    Id = u.Id
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                return Result<LoginQueryResponse>.Failure("User not found");

            var decryptedPassword = _encryption.Decrypt(user.Password);

            if (decryptedPassword != request.password)
                return Result<LoginQueryResponse>.Failure("Invalid username or password");

            user.Password = null;

            return Result<LoginQueryResponse>.Success(user);
        }
    }
}
