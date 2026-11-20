using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Parameters.Queries.GetChildMenu
{
    public sealed record GetChildMenuQuery(int MasterParameterId) : IRequest<Result<IReadOnlyList<GlParameterResponse>>>;

    internal sealed class GetChildMenuQueryHandler : IRequestHandler<GetChildMenuQuery, Result<IReadOnlyList<GlParameterResponse>>>
    {
        private readonly ITemplateDbContext _context;

        public GetChildMenuQueryHandler(ITemplateDbContext context)
        {
            _context = context;
        }

        public async ValueTask<Result<IReadOnlyList<GlParameterResponse>>> Handle(GetChildMenuQuery request,
                                                                                  CancellationToken cancellationToken)
        {
            var childParameters = await _context.GlParameters.AsNoTracking()
                .Where(p => p.ParameterTypeId == request.MasterParameterId && p.IsDeleted == 0)
                .OrderBy(p => p.Sort)
                .Select(p => new GlParameterResponse(
                    p.GlParameterId,
                    p.ParameterId,
                    p.ParameterCode,
                    p.ParameterTypeId,
                    p.ParameterDescription,
                    p.ParameterDescriptionEn,
                    p.Sort))
                .ToListAsync(cancellationToken);

            if (childParameters == null || !childParameters.Any())
            {
                return Result<IReadOnlyList<GlParameterResponse>>.Success(Array.Empty<GlParameterResponse>());
            }

            return Result<IReadOnlyList<GlParameterResponse>>.Success(childParameters);
        }
    }
}
