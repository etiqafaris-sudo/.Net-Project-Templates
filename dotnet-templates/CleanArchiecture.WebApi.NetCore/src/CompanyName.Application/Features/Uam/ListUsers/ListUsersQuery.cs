using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

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

public sealed record UserListItemDto(
    int Id,
    string Email,
    string FullName,
    UserStatus Status,
    byte[] RowVersion);

internal sealed class ListUsersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ListUsersQuery, Result<PagedResult<UserListItemDto>>>
{
    public async ValueTask<Result<PagedResult<UserListItemDto>>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page;
        var pageSize = request.PageSize;
        var skip = (page - 1) * pageSize;

        var usersQuery = context.Users.AsNoTracking();

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var users = await usersQuery
            .OrderBy(u => u.FullName)
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new UserListItemDto(u.Id, u.Email, u.FullName, u.Status, u.RowVersion))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<UserListItemDto>(users, page, pageSize, totalCount));
    }
}