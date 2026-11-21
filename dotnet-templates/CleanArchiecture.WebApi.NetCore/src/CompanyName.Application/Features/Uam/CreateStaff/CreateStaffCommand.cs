using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Uam.CreateStaff;

public sealed record CreateStaffCommand(string Email, string FullName) : IRequest<Result<int>>;

public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(256);
    }
}

internal sealed class CreateStaffCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateStaffCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            return Result.Failure<int>("A user with this email already exists.");
        }

        var user = User.Create(normalizedEmail, request.FullName.Trim());
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}