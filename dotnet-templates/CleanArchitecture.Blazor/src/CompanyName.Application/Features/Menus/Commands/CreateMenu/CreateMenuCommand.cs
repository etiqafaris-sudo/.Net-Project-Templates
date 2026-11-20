using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Menus.Events;
using CompanyName.Domain.Entities;
using CompanyName.Shared;
using Mediator;

namespace CompanyName.Application.Features.Menus.Commands.CreateMenu
{
    public sealed record CreateMenuCommand(string? MenuName, string? MenuUrl, string? Icon, int? ParentId, int? OrderNo, int IsActive) : IRequest<Result<int>>;

    internal sealed class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Result<int>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public CreateMenuCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<int>> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = new GlPortalMenu
            {
                MenuName = request.MenuName,
                MenuUrl = request.MenuUrl,
                Icon = request.Icon,
                ParentId = request.ParentId,
                OrderNo = request.OrderNo,
                IsActive = request.IsActive,
                IsDeleted = 0
            };
            menu.SetAuditable(Domain.Enums.OpTypeEnum.Create);
            await _context.GlPortalMenus.AddAsync(menu, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new MenuUpdateEvent());
            return Result.Success(menu.Id);
        }
    }
}
