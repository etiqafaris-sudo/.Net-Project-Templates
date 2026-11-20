using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using FluentValidation;
using Mediator;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

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

internal sealed class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOutbox _outbox;

    public CreateStaffCommandHandler(IApplicationDbContext context, IOutbox outbox)
    {
        _context = context;
        _outbox = outbox;
    }

    public async ValueTask<Result<int>> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            return Result.Failure<int>("A user with this email already exists.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            FullName = request.FullName.Trim(),
            Status = UserStatus.Active,
        };

        _context.Users.Add(user);
        _outbox.Enqueue(new StaffCreatedEvent(user.Id, user.Email, user.FullName));
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}