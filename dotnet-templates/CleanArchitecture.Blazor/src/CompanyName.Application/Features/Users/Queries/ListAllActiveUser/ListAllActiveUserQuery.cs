using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Users.Queries.ListAllUser;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Queries.ListAllActiveUser
{
    public sealed record ListAllActiveUserQuery : IRequest<Result<IReadOnlyList<ListAllUserResponse>>>;

    internal sealed class ListAllActiveUserQueryHandler : IRequestHandler<ListAllActiveUserQuery, Result<IReadOnlyList<ListAllUserResponse>>>
    {
        private readonly ITemplateDbContext _context;

        public ListAllActiveUserQueryHandler(ITemplateDbContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<IReadOnlyList<ListAllUserResponse>>> Handle(ListAllActiveUserQuery request, CancellationToken cancellationToken)
        {
            var users = await _context.GlUsers.AsNoTracking()
                 .Where(u => u.IsDeleted == 0 && u.IsActive == 1)
                 .Select(u => new ListAllUserResponse
                 {
                     Id = u.Id,
                     Email = u.Email,
                     Username = u.Username,
                     Fullname = u.Fullname,
                     Password = u.Password,
                     IcNo = u.IcNo,
                     Role = u.Role,
                     IsActive = u.IsActive == 1
                 })
                 .ToListAsync(cancellationToken);
            return Result.Success<IReadOnlyList<ListAllUserResponse>>(users);
        }
    }
}
