using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Parameters.EventHandlers;
using CompanyName.Domain.Entities;
using CompanyName.Domain.Enums;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Parameters.Commands.CreateParameterDesc
{
    public sealed record CreateParameterDescCommand : IRequest<Result<bool>>
    {
        public GLParameterDescRequest GLParameterDesc { get; init; } = default!;
        public IReadOnlyList<GLParameterRequest> Children { get; init; } = Array.Empty<GLParameterRequest>();
    }

    internal sealed class CreateParameterDescCommandHandler : IRequestHandler<CreateParameterDescCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public CreateParameterDescCommandHandler(ITemplateDbContext context,IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(CreateParameterDescCommand request,
                                                    CancellationToken cancellationToken)
        {
            var desc = request.GLParameterDesc;

            try
            {
                var parameterTypeId = desc.ParameterTypeId;

                if (parameterTypeId == 0)
                {
                    var masterEntity = new GlParametersDescription
                    {
                        ParameterDescription = desc.ParameterDescription,
                        Sort = desc.Sort,
                        IsDeleted = 0,
                        IsActive = 1
                    };
                    masterEntity.SetAuditable(OpTypeEnum.Create);
                    await _context.GlParametersDescriptions.AddAsync(masterEntity, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    parameterTypeId = masterEntity.ParameterTypeId;
                    masterEntity.ParameterId = parameterTypeId.ToString();
                    masterEntity.SetAuditable(OpTypeEnum.Update);
                }
                else
                {
                    var masterInDb = await _context.GlParametersDescriptions.FirstOrDefaultAsync(
                        x => x.ParameterTypeId == parameterTypeId,
                        cancellationToken);

                    if (masterInDb is null)
                    {
                        return Result<bool>.Failure("Parameter master not found.");
                    }

                    masterInDb.ParameterDescription = desc.ParameterDescription;
                    masterInDb.Sort = desc.Sort;
                    masterInDb.IsDeleted = 0;
                    masterInDb.SetAuditable(OpTypeEnum.Update);
                }

                var existingChildren = await _context.GlParameters
                    .Where(p => p.ParameterTypeId == parameterTypeId && p.IsDeleted == 0)
                    .ToListAsync(cancellationToken);

                foreach (var existingChild in existingChildren)
                {
                    if (!request.Children.Any(d => d.ParameterId == existingChild.ParameterId))
                    {
                        existingChild.IsDeleted = 1;
                        existingChild.SetAuditable(OpTypeEnum.Update);
                    }
                }

                foreach (var detail in request.Children)
                {
                    var existingChild = existingChildren.FirstOrDefault(c => c.ParameterId == detail.ParameterId);
                    if (existingChild is not null)
                    {
                        existingChild.ParameterDescription = detail.ParameterDescription;
                        existingChild.ParameterDescriptionEn = detail.ParameterDescriptionEn;
                        existingChild.Sort = detail.Sort;
                        existingChild.ParameterCode = detail.ParameterId;
                        existingChild.IsDeleted = 0;
                        existingChild.SetAuditable(OpTypeEnum.Update);
                    }
                    else
                    {
                        var newChild = new GlParameter
                        {
                            ParameterId = detail.ParameterId,
                            ParameterCode = detail.ParameterId,
                            ParameterTypeId = parameterTypeId,
                            ParameterDescription = detail.ParameterDescription,
                            ParameterDescriptionEn = detail.ParameterDescriptionEn,
                            Sort = detail.Sort,
                            IsDeleted = 0,
                            IsActive = 1
                        };
                        newChild.SetAuditable(OpTypeEnum.Create);
                        await _context.GlParameters.AddAsync(newChild, cancellationToken);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await _mediator.Publish(new ParameterDescChangeEvent());
                return Result<bool>.Success(true);
            }
            catch
            {
                return Result<bool>.Failure("An error occurred while saving the parameter.");
            }
        }
    }

    public sealed record GLParameterDescRequest(int ParameterTypeId, string? ParameterDescription, int? Sort);

    public sealed record GLParameterRequest(
        int GlParameterId,
        string ParameterId,
        string? ParameterCode,
        int ParameterTypeId,
        string? ParameterDescription,
        string? ParameterDescriptionEn,
        int? Sort);
}
