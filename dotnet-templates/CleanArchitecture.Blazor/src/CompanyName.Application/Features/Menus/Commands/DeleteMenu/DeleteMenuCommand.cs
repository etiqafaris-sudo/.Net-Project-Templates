using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Menus.Events;
using CompanyName.Shared;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Features.Menus.Commands.DeleteMenu
{
    public sealed record DeleteMenuCommand(int menuId) : IRequest<Result<bool>>;

    internal sealed class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, Result<bool>>
    {
        private readonly ITemplateDbContext _applicationDbContext;
        private readonly IMediator _mediator;

        public DeleteMenuCommandHandler(ITemplateDbContext applicationDbContext, IMediator mediator)
        {
            _applicationDbContext = applicationDbContext;
            _mediator = mediator;
        }

        public async ValueTask<Result<bool>> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var menu = await _applicationDbContext.GlPortalMenus.FirstOrDefaultAsync(x => x.Id == request.menuId, cancellationToken);
                if (menu is null)
                    return Result.Failure<bool>("Menu not found.");

                menu.IsDeleted = 1; // Mark as deleted
                menu.IsActive = 0; // Mark as inactive
                menu.SetAuditable(Domain.Enums.OpTypeEnum.Delete);
                _applicationDbContext.GlPortalMenus.Update(menu);
                await _applicationDbContext.SaveChangesAsync(cancellationToken);

                await _mediator.Publish(new MenuUpdateEvent());
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>(ex.Message);
            }
        }
    }
}
