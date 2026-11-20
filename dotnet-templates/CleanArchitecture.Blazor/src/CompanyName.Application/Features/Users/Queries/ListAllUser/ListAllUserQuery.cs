using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Users.Queries.ListAllUser
{
    //todo: Faris Consider adding pagination and filtering to this query for better performance with large user lists.
    public sealed record ListAllUserQuery : IRequest<Result<IReadOnlyList<ListAllUserResponse>>>;

    internal sealed class ListAllUserQueryHandler : IRequestHandler<ListAllUserQuery, Result<IReadOnlyList<ListAllUserResponse>>>
    {
        private readonly ITemplateDbContext _context;
        public ListAllUserQueryHandler(ITemplateDbContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<IReadOnlyList<ListAllUserResponse>>> Handle(ListAllUserQuery request,
                                                                                  CancellationToken cancellationToken)
        {
            var users = await _context.GlUsers.AsNoTracking()
                .Where(u => u.IsDeleted == 0)
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

    public sealed class ListAllUserResponse
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
}
