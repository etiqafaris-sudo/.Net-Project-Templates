using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Samples.Commands.DeleteUserSample
{
    public sealed record DeleteUserSampleCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    internal sealed class DeleteUserSampleCommandHandler : IRequestHandler<DeleteUserSampleCommand, Result<bool>>
    {
        private readonly ITestingDbContext _testingDbContext;

        public DeleteUserSampleCommandHandler(ITestingDbContext testingDbContext)
        {
            _testingDbContext = testingDbContext;
        }

        public async ValueTask<Result<bool>> Handle(DeleteUserSampleCommand request, CancellationToken cancellationToken)
        {
            var user = await _testingDbContext.TblUsers.Where(x => x.Id == request.Id)
                                                 .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result<bool>.Failure("User not exists");
            }

            user.IsDeleted = 1;
            user.SetAuditable(Domain.Enums.OpTypeEnum.Delete);

            _testingDbContext.TblUsers.Update(user);
            await _testingDbContext.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
