using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Parameters.EventHandlers;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Parameters.Commands.ToggleParameterDesc
{
    public sealed record ToggleParameterDescCommand(int ParameterTypeId) : IRequest<Result<bool>>;

    internal sealed class ToggleParameterDescCommandHandler : IRequestHandler<ToggleParameterDescCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public ToggleParameterDescCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(ToggleParameterDescCommand request, CancellationToken cancellationToken)
        {
            var parameter = await _context.GlParametersDescriptions.FirstOrDefaultAsync(p => p.ParameterTypeId == request.ParameterTypeId,
                                                                                        cancellationToken);
            if (parameter is null)
            {
                return Result.Failure<bool>($"Parameter with ID {request.ParameterTypeId} not found.");
            }

            parameter.IsActive = parameter.IsActive == 0 ? 1 : 0;
            parameter.SetAuditable(Domain.Enums.OpTypeEnum.Update);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new ParameterDescChangeEvent());
            return Result.Success(parameter.IsActive == 1);
        }
    }
}
