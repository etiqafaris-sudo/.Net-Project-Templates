using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Samples.Commands.UpdateUser
{
    public sealed record UpdateUserTestingCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }

    internal sealed class UpdateUserTestingCommandHandler : IRequestHandler<UpdateUserTestingCommand, Unit>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ITestingDbContext _testingDbContext;

        public UpdateUserTestingCommandHandler(IDbConnectionFactory dbConnectionFactory,
                                        ITestingDbContext testingDbContext)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _testingDbContext = testingDbContext;
        }

        public async ValueTask<Unit> Handle(UpdateUserTestingCommand request, CancellationToken cancellationToken)
        {
            var user = await _testingDbContext.TblUsers
                            .Where(x => x.Id == request.Id)
                            .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                throw new InvalidOperationException("User not found");

            user.Email = request.Email;
            user.Name = request.Name;
            user.SetAuditable(OpTypeEnum.Update);

            _testingDbContext.TblUsers.Update(user);
            await _testingDbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
