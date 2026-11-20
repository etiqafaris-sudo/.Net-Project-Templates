using CompanyName.Application.Features.Menus.Commands.CreateMenu;
using CompanyName.Application.Features.Menus.Commands.UpdateMenu;
using CompanyName.Application.Features.Menus.Queries.GetParentMenu;
using CompanyName.Web.Models.Settings;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class AddPortalMenuDialog : AppComponentBase<GLPortalMenuDto>
{
    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Inject]
    public IMediator Mediator { get; set; }

    [Parameter]
    public GLPortalMenuDto? Menu { get; set; }

    protected IReadOnlyList<GLPortalMenuDto>? parentMenus;

    // UI boolean binding for IsActive switch
    public bool IsActiveBool
    {
        get => Model.IsActive == 1;
        set => Model.IsActive = value ? 1 : 0;
    }

    protected override async Task OnInitializedAsync()
    {
        parentMenus = Array.Empty<GLPortalMenuDto>();
        var parentMenuResult = await Mediator.Send(new GetParentMenuQuery());
        if (Menu != null)
        {
            Model = Menu;
            var elligible = parentMenuResult.Value.Where(m => m.Id != Menu.Id).ToList();
            parentMenus = elligible.Select(m => new GLPortalMenuDto
            {
                Id = m.Id,
                MenuName = m.MenuName,
                MenuUrl = m.MenuUrl,
                Icon = m.Icon,
                OrderNo = m.OrderNo,
                ParentId = m.ParentId,
                IsActive = m.IsActive.Value
            }).ToList();
        }
        else
        {
            Model = new GLPortalMenuDto
            {
                Id = 0,
                MenuName = string.Empty,
                MenuUrl = string.Empty,
                Icon = string.Empty,
                ParentId = null,
                OrderNo = null,
                IsActive = 1 // Default to active
            };

            Menu = Model;
            parentMenus = parentMenuResult.Value.Select(m => new GLPortalMenuDto
            {
                Id = m.Id,
                MenuName = m.MenuName,
                MenuUrl = m.MenuUrl,
                Icon = m.Icon,
                OrderNo = m.OrderNo,
                ParentId = m.ParentId,
                IsActive = m.IsActive.Value
            }).ToList();
        }
    }

    public async Task OnSubmit()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Menu.MenuName))
            {
                await AlertService.JSWarningAsync("Menu Name is required.");
                return;
            }

            if (Menu.Id == 0)
            {
                var result = await Mediator.Send(new CreateMenuCommand(
                    MenuName: Menu.MenuName,
                    MenuUrl: Menu.MenuUrl,
                    Icon: Menu.Icon,
                    ParentId: Menu.ParentId,
                    OrderNo: Menu.OrderNo,
                    IsActive: Menu.IsActive
                ));

                if (result.IsFailure)
                {
                    await AlertService.JSErrorAsync($"Failed to create menu: {string.Join(", ", result.Errors)}");
                    return;
                }

                await AlertService.JSSuccessAsync("Menu created successfully");
            }
            else
            {
                var result = await Mediator.Send(new UpdateMenuCommand(
                    Id: Menu.Id,
                    MenuName: Menu.MenuName,
                    MenuUrl: Menu?.MenuUrl,
                    Icon: Menu?.Icon,
                    ParentId: Menu.ParentId,
                    OrderNo: Menu.OrderNo,
                    IsActive: Menu.IsActive
                ));

                if (result.IsFailure)
                {
                    await AlertService.JSErrorAsync($"Failed to update menu: {string.Join(", ", result.Errors)}");
                    return;
                }

                await AlertService.JSSuccessAsync("Menu updated successfully");
            }

            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            await AlertService.JSErrorAsync($"Failed to save menu: {ex.Message}");
        }
    }
}
