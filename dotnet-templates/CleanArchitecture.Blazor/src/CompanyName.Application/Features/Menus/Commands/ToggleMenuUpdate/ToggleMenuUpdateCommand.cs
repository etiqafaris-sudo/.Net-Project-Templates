using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Menus.Events;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Menus.Commands.ToggleMenuUpdate
{
    public sealed record ToggleMenuUpdateCommand(int menuId, int isActive) : IRequest<Result<bool>>;

    internal sealed class ToggleMenuUpdateCommandHandler : IRequestHandler<ToggleMenuUpdateCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _context;
        private readonly IMediator _mediator;

        public ToggleMenuUpdateCommandHandler(ITemplateDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(ToggleMenuUpdateCommand request, CancellationToken cancellationToken)
        {
            var menu = await _context.GlPortalMenus.FirstOrDefaultAsync(x => x.Id == request.menuId, cancellationToken);
            if (menu is null)
            {
                return Result.Failure<bool>("Menu not found.");
            }

            menu.IsActive = request.isActive;
            menu.SetAuditable(Domain.Enums.OpTypeEnum.Update);
            _context.GlPortalMenus.Update(menu);
            await _context.SaveChangesAsync(cancellationToken);
            await _mediator.Publish(new MenuUpdateEvent());
            return Result.Success(true);
        }
    }
}
