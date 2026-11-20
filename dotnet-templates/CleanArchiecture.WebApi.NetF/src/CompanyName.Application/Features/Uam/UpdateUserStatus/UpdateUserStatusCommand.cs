using CompanyName.Application.Common.Exceptions;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using FluentValidation;
using Mediator;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

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

internal sealed class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IOutbox _outbox;

    public UpdateUserStatusCommandHandler(IApplicationDbContext context, IOutbox outbox)
    {
        _context = context;
        _outbox = outbox;
    }

    public async ValueTask<Result> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        _context.MarkRowVersion(user, request.RowVersion);
        user.Status = request.Status;
        _outbox.Enqueue(new UpdatedUserStatusEvent(user.Id));
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException();
        }
    }
}