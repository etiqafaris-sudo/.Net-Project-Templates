using CompanyName.Application.Features.Menus.Commands.DeleteMenu;
using CompanyName.Application.Features.Menus.Commands.ToggleMenuUpdate;
using CompanyName.Application.Features.Menus.Queries.GetAllMenus;
using CompanyName.Web.Models.Settings;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class PortalMenus : AppComponentBase<GLPortalMenuDto>
{
    [Inject]
    public IMediator Mediator { get; set; }
    private IReadOnlyList<GLPortalMenuDto>? allMenus;
    private IReadOnlyList<GLPortalMenuDto>? menus;
    private string searchText = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        allMenus = Array.Empty<GLPortalMenuDto>();
        menus = Array.Empty<GLPortalMenuDto>();
        await LoadMenus();
    }

    private async Task LoadMenus()
    {
        await RunWithLoadingAsync(async () =>
        {
            var result = await Mediator.Send(new GetAllMenusQuery());
            allMenus = result.Value.Select(m => new GLPortalMenuDto
            {
                Id = m.Id,
                MenuName = m.MenuName,
                MenuUrl = m.MenuUrl,
                Icon = m.Icon,
                ParentId = m.ParentId,
                OrderNo = m.OrderNo,
                IsActive = m.IsActive.Value
            }).ToList();
            ApplyFilter();
        });
    }

    private void OnSearch(ChangeEventArgs args)
    {
        searchText = args.Value?.ToString() ?? string.Empty;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            menus = BuildHierarchy(allMenus);
            return;
        }

        var lowerSearch = searchText.ToLower();

        var filtered = allMenus?.Where(u =>
            (u.MenuName != null && u.MenuName.ToLower().Contains(lowerSearch)) ||
            (u.MenuUrl != null && u.MenuUrl.ToLower().Contains(lowerSearch)) ||
            (u.Icon != null && u.Icon.ToLower().Contains(lowerSearch)) ||
            GetActiveStatus(u.IsActive).ToLower().Contains(lowerSearch)
        ).ToList();

        menus = BuildHierarchy(filtered);
    }

    private IReadOnlyList<GLPortalMenuDto>? BuildHierarchy(IReadOnlyList<GLPortalMenuDto>? flatList)
    {
        if (flatList == null) return null;

        var result = new List<GLPortalMenuDto>();

        // Find main menus (ParentId is null or 0)
        var mains = flatList.Where(m => m.ParentId == null || m.ParentId == 0).OrderBy(m => m.OrderNo).ToList();

        foreach (var main in mains)
        {
            result.Add(main);
            // Append children beneath each main menu logically
            var children = flatList.Where(m => m.ParentId == main.Id).OrderBy(m => m.OrderNo).ToList();
            foreach (var child in children)
            {
                result.Add(child);
            }
        }

        // Add any orphans that might exist (children without matched parents in current filter sequence)
        var processedIds = result.Select(r => r.Id).ToHashSet();
        var orphans = flatList.Where(m => !processedIds.Contains(m.Id)).ToList();
        result.AddRange(orphans);

        return result;
    }

    private string GetActiveStatus(int? isActive)
    {
        return isActive == 1 ? L.GetLangValue("lbl_active_yes") : L.GetLangValue("lbl_active_no");
    }

    private async Task AddMenu()
    {
        var result = await DialogService.OpenAsync<AddPortalMenuDialog>("Add Menu",
               new Dictionary<string, object?>(),
               new DialogOptions() { Width = "800px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadMenus();
        }
    }

    private async Task EditMenu(GLPortalMenuDto menuToEdit)
    {
        var result = await DialogService.OpenAsync<AddPortalMenuDialog>("Edit Menu",
              new Dictionary<string, object?> { { "Menu", menuToEdit } },
              new DialogOptions() { Width = "800px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadMenus();
        }
    }

    private async Task DeleteMenu(GLPortalMenuDto menuToDelete)
    {
        var confirmResult = await DialogService.Confirm(
            L.GetLangValue("lbl_confirm_delete"),
            L.GetLangValue("lbl_confirm_delete_title"),
            new ConfirmOptions() { OkButtonText = L.GetLangValue("lbl_yes"), CancelButtonText = L.GetLangValue("lbl_cancel") }
        );

        if (confirmResult == true)
        {
            var result = await Mediator.Send(new DeleteMenuCommand(menuToDelete.Id));

            if (result.IsFailure)
            {
                await AlertService.JSErrorAsync(result.Errors.FirstOrDefault() ?? L.GetLangValue("lbl_delete_error"));
                return;
            }

            await AlertService.JSSuccessAsync(L.GetLangValue("lbl_delete_success"));
            await LoadMenus();
        }
    }

    private async Task ToggleMenuStatus(GLPortalMenuDto menu)
    {
        bool isCurrentlyActive = menu.IsActive == 1;
        string confirmMessage = isCurrentlyActive
            ? L.GetLangValue("lbl_confirm_deactivate")
            : L.GetLangValue("lbl_confirm_activate");

        var confirmResult = await DialogService.Confirm(
            confirmMessage,
            L.GetLangValue("lbl_confirm_status_title"),
            new ConfirmOptions() { OkButtonText = L.GetLangValue("lbl_yes"), CancelButtonText = L.GetLangValue("lbl_cancel") }
        );

        if (confirmResult == true)
        {
            menu.IsActive = isCurrentlyActive ? 0 : 1;

            var result = await Mediator.Send(new ToggleMenuUpdateCommand(menu.Id, menu.IsActive));
            if (result.IsFailure)
            {
                await AlertService.JSErrorAsync(L.GetLangValue("lbl_update_error"));
                return;
            }

            await AlertService.JSSuccessAsync(L.GetLangValue("lbl_update_success"));
            await LoadMenus();
        }
    }
}
