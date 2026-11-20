using CompanyName.Application.Features.Users.Commands.DeleteUserGroup;
using CompanyName.Application.Features.Users.Commands.ToggleUserGroup;
using CompanyName.Application.Features.Users.Queries.GetUserGroup;
using CompanyName.Web.Models.Users;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class UserGroups : AppComponentBase<GlUserGroupDto>
{
    [Inject]
    private IMediator _mediator { get; set; } = default!;
    private IReadOnlyList<GlUserGroupDto>? allGroups;
    private IReadOnlyList<GlUserGroupDto>? groups;
    private string searchText = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadGroups();
    }

    private async Task LoadGroups()
    {
        await RunWithLoadingAsync(async () =>
        {
            allGroups = Array.Empty<GlUserGroupDto>();
            groups = Array.Empty<GlUserGroupDto>();

            var result = await _mediator.Send(new GetUserGroupQuery());
            if (result.IsSuccess)
            {
                allGroups = result.Value.Select(g => new GlUserGroupDto
                {
                    Id = g.Id,
                    GroupCode = g.GroupCode,
                    GroupName = g.GroupName,
                    IsActive = g.IsActive,
                }).ToList();
            }

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
            groups = allGroups;
            return;
        }

        var lowerSearch = searchText.ToLower();

        groups = allGroups?.Where(u =>
            (u.GroupCode != null && u.GroupCode.ToLower().Contains(lowerSearch)) ||
            (u.GroupName != null && u.GroupName.ToLower().Contains(lowerSearch)) ||
            GetActiveStatus(u.IsActive).ToLower().Contains(lowerSearch)
        ).ToList();
    }

    private string GetActiveStatus(int? isActive)
    {
        return isActive == 1 ? L.GetLangValue("lbl_active_yes") : L.GetLangValue("lbl_active_no");
    }

    private async Task AddGroup()
    {
        var result = await DialogService.OpenAsync<AddUserGroupDialog>("Add Group",
               new Dictionary<string, object?>(),
               new DialogOptions() { Width = "600px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadGroups();
        }
    }

    private async Task EditGroup(GlUserGroupDto groupToEdit)
    {
        var result = await DialogService.OpenAsync<AddUserGroupDialog>("Edit Group",
              new Dictionary<string, object?> { { "Group", groupToEdit } },
              new DialogOptions() { Width = "600px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadGroups();
        }
    }

    private async Task ManageMenus(GlUserGroupDto group)
    {
        var result = await DialogService.OpenAsync<ManageGroupMenusDialog>($"Manage Menus: {group.GroupName}",
              new Dictionary<string, object?> { { "Group", group } },
              new DialogOptions() { Width = "600px", Height = "auto", Resizable = false, Draggable = false });
    }

    private async Task AssignUsers(GlUserGroupDto group)
    {
        var result = await DialogService.OpenAsync<AssignGroupUsersDialog>($"Assign Users: {group.GroupName}",
              new Dictionary<string, object?> { { "Group", group } },
              new DialogOptions() { Width = "600px", Height = "auto", Resizable = false, Draggable = false });
    }

    private async Task DeleteGroup(GlUserGroupDto groupToDelete)
    {
        var confirmResult = await DialogService.Confirm(
            L.GetLangValue("lbl_confirm_delete"),
            L.GetLangValue("lbl_confirm_delete_title"),
            new ConfirmOptions() { OkButtonText = L.GetLangValue("lbl_yes"), CancelButtonText = L.GetLangValue("lbl_cancel") }
        );

        if (confirmResult != true)
        {
            return;
        }

        var result = await _mediator.Send(new DeleteUserGroupCommand
        {
            GroupId = groupToDelete.Id,
        });

        if (result.IsFailure)
        {
            await AlertService.JSErrorAsync(L.GetLangValue("lbl_delete_error"));
            return;
        }

        await AlertService.JSSuccessAsync(L.GetLangValue("lbl_delete_success"));
        await LoadGroups();
    }

    private async Task ToggleGroupStatus(GlUserGroupDto group)
    {
        bool isCurrentlyActive = group.IsActive == 1;
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
            var result = await _mediator.Send(new ToggleUserGroupCommand(group.Id));
            if (result.IsFailure)
            {
                await AlertService.JSErrorAsync(L.GetLangValue("lbl_update_error"));
                return;
            }

            await AlertService.JSSuccessAsync(L.GetLangValue("lbl_update_success"));
            await LoadGroups();
        }
    }
}
