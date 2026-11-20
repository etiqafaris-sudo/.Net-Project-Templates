using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Menus.Queries.GetParentMenu
{
    public sealed record GetParentMenuQuery : IRequest<Result<IReadOnlyList<GetAllMenusReponse>>>;

    internal sealed class GetParentMenuQueryHandler : IRequestHandler<GetParentMenuQuery, Result<IReadOnlyList<GetAllMenusReponse>>>
    {
        private readonly ITemplateDbContext _context;
        public GetParentMenuQueryHandler(ITemplateDbContext context)
        {
            _context = context;
        }
        public async ValueTask<Result<IReadOnlyList<GetAllMenusReponse>>> Handle(GetParentMenuQuery request, CancellationToken cancellationToken)
        {
            var menus = await _context.GlPortalMenus.Where(m => m.IsDeleted == 0
                                                                && (m.ParentId == null || m.ParentId == 0))
                                                    .Select(x => new GetAllMenusReponse
                                                    {
                                                        Id = x.Id,
                                                        MenuName = x.MenuName,
                                                        MenuUrl = x.MenuUrl,
                                                        Icon = x.Icon,
                                                        ParentId = x.ParentId,
                                                        OrderNo = x.OrderNo,
                                                        IsActive = x.IsActive.Value
                                                    })
                                                    .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<GetAllMenusReponse>>.Success(menus);
        }
    }
}
