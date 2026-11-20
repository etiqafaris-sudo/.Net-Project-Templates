using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Parameters.EventHandlers;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Parameters.Commands.DeleteParameterDesc
{
    public sealed record DeleteParameterDescCommand(int ParameterTypeId) : IRequest<Result<bool>>;

    internal sealed class DeleteParameterDescCommandHandler : IRequestHandler<DeleteParameterDescCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public DeleteParameterDescCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(DeleteParameterDescCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.GlParametersDescriptions.FirstOrDefaultAsync(x => x.ParameterTypeId == request.ParameterTypeId
                                                                                          && x.IsDeleted == 0, cancellationToken);
            if (entity is null)
            {
                return Result.Failure<bool>($"Parameter with ID {request.ParameterTypeId} not found.");
            }

            entity.IsDeleted = 1; // Mark as deleted
            entity.SetAuditable(Domain.Enums.OpTypeEnum.Delete);
            _context.GlParametersDescriptions.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new ParameterDescChangeEvent());
            return Result.Success(true);
        }
    }
}
