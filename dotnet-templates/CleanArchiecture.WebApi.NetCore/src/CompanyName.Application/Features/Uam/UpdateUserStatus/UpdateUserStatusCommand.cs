using CompanyName.Application.Common.Exceptions;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Uam.UpdateUserStatus;

public sealed record UpdateUserStatusCommand(int Id, UserStatus Status, byte[] RowVersion) : IRequest<Result>;

public sealed class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.RowVersion).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

internal sealed class UpdateUserStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateUserStatusCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        context.MarkRowVersion(user, request.RowVersion);
        user.ChangeStatus(request.Status);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException();
        }
    }
}