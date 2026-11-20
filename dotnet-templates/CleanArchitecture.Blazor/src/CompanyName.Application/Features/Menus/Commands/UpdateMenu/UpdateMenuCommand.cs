using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Menus.Events;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Menus.Commands.UpdateMenu
{
    public sealed record UpdateMenuCommand(int Id, string MenuName, string MenuUrl, string Icon, int? ParentId, int? OrderNo, int IsActive) : IRequest<Result<bool>>;

    internal sealed class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public UpdateMenuCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = await _context.GlPortalMenus.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (menu is null)
            {
                return Result.Failure<bool>($"Menu with ID {request.Id} not found.");
            }

            menu.MenuName = request.MenuName;
            menu.MenuUrl = request.MenuUrl;
            menu.Icon = request.Icon;
            menu.ParentId = request.ParentId;
            menu.OrderNo = request.OrderNo;
            menu.IsActive = request.IsActive;
            menu.SetAuditable(Domain.Enums.OpTypeEnum.Update);
            _context.GlPortalMenus.Update(menu);
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new MenuUpdateEvent());
            return Result.Success(true);
        }
    }
}
