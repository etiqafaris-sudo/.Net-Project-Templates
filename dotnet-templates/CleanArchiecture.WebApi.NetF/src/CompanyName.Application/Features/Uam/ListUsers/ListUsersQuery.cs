using System.Data.Entity;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using FluentValidation;
using Mediator;

namespace CompanyName.Application.Features.Uam.ListUsers;

public sealed record ListUsersQuery(int Page = 1, int PageSize = 50) : IRequest<Result<PagedResult<UserListItemDto>>>;

public sealed class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    public const int MaxPageSize = 100;

    public ListUsersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}

public sealed class UserListItemDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

internal sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, Result<PagedResult<UserListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public ListUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<PagedResult<UserListItemDto>>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page;
        var pageSize = request.PageSize;
        var skip = (page - 1) * pageSize;

        var usersQuery = _context.Users.AsNoTracking();

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var users = await usersQuery
            .OrderBy(u => u.FullName)
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new UserListItemDto
            {
                Email = u.Email,
                FullName = u.FullName,
                Id = u.Id,
                Status = u.Status,
                RowVersion = u.RowVersion,
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<UserListItemDto>(users, page, pageSize, totalCount));
    }
}
