using CompanyName.Application.Features.Users.Commands.DeactivateUser;
using CompanyName.Application.Features.Users.Commands.DeleteUser;
using CompanyName.Application.Features.Users.Queries.GetUserGroup;
using CompanyName.Application.Features.Users.Queries.ListAllUser;
using CompanyName.Domain.Entities;
using CompanyName.Web.Models.Users;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class Users : AppComponentBase<UserDto>
{
    [Inject]
    IMediator Mediator { get; set; } = default!;

    private IEnumerable<UserDto>? allUsers;
    private IEnumerable<UserDto>? users;
    private string searchText = string.Empty;
    private Dictionary<string, string> userGroupNamesById = new(StringComparer.Ordinal);

    protected override async Task OnInitializedAsync()
    {
        await RunWithLoadingAsync(async () =>
        {
            await LoadUserGroupNamesAsync();
            await LoadUsers();
        });
    }

    private async Task LoadUserGroupNamesAsync()
    {
        var result = await Mediator.Send(new GetUserGroupQuery());
        if (result.IsFailure)
        {
            userGroupNamesById = new Dictionary<string, string>(StringComparer.Ordinal);
            return;
        }

        userGroupNamesById = result.Value
            .ToDictionary(g => g.Id.ToString(), g => g.GroupName ?? g.GroupCode ?? g.Id.ToString(), StringComparer.Ordinal);
    }

    private async Task LoadUsers()
    {
        await RunWithLoadingAsync(async () =>
        {
            var results = await Mediator.Send(new ListAllUserQuery());

            if (results.IsFailure)
            {
                await AlertService.JSErrorAsync(L.GetLangValue("lbl_load_error"));
                return;
            }

            allUsers = results.Value.Select(MapToUserDto).ToList();
            ApplyFilter();
        });
    }

    private static UserDto MapToUserDto(ListAllUserResponse user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Username = user.Username,
        Fullname = user.Fullname,
        IcNo = user.IcNo,
        Role = user.Role,
        IsActive = user.IsActive
    };

    private static GlUser MapToGlUser(UserDto user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Username = user.Username,
        Fullname = user.Fullname,
        IcNo = user.IcNo,
        Role = user.Role,
        IsActive = user.IsActive ? 1 : 0
    };

    private void OnSearch(ChangeEventArgs args)
    {
        searchText = args.Value?.ToString() ?? "";
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            users = allUsers;
            return;
        }

        var lowerSearch = searchText.ToLower();

        users = allUsers?.Where(u =>
            (u.Fullname != null && u.Fullname.ToLower().Contains(lowerSearch)) ||
            (u.Email != null && u.Email.ToLower().Contains(lowerSearch)) ||
            GetRoleName(u.Role).ToLower().Contains(lowerSearch) ||
            GetActiveStatus(u.IsActive).ToLower().Contains(lowerSearch)
        ).ToList();
    }

    private string GetRoleName(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return string.Empty;
        }

        return userGroupNamesById.TryGetValue(role, out var groupName) ? groupName : string.Empty;
    }

    private string GetActiveStatus(bool isActive)
    {
        return isActive ? L.GetLangValue("lbl_active_yes") : L.GetLangValue("lbl_active_no");
    }

    private async Task AddUser()
    {
        var result = await DialogService.OpenAsync<AddUserDialog>("Add User",
               new Dictionary<string, object?>(),
               new DialogOptions() { Width = "1000px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadUsers();
        }
    }

    private async Task EditUser(UserDto userToEdit)
    {
        var result = await DialogService.OpenAsync<AddUserDialog>("Edit User",
              new Dictionary<string, object?> { { "User", userToEdit } },
              new DialogOptions() { Width = "1000px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadUsers();
        }
    }

    private async Task DeleteUser(UserDto userToDelete)
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

        var result = await Mediator.Send(new DeleteUserCommand
        {
            UserId = userToDelete.Id,
        });

        if (result.IsFailure)
        {
            await AlertService.JSErrorAsync(L.GetLangValue("lbl_delete_error"));
            return;
        }

        await AlertService.JSSuccessAsync(L.GetLangValue("lbl_delete_success"));
        await LoadUsers();
    }

    private async Task ToggleUserStatus(UserDto user)
    {
        string confirmMessage = user.IsActive
            ? L.GetLangValue("lbl_confirm_deactivate")
            : L.GetLangValue("lbl_confirm_activate");

        var confirmResult = await DialogService.Confirm(
            confirmMessage,
            L.GetLangValue("lbl_confirm_status_title"),
            new ConfirmOptions() { OkButtonText = L.GetLangValue("lbl_yes"), CancelButtonText = L.GetLangValue("lbl_cancel") }
        );

        if (confirmResult != true)
        {
            return;
        }

        user.IsActive = !user.IsActive;

        var result = await Mediator.Send(new DeactivateUserCommand
        {
            UserId = user.Id,
            IsActive = user.IsActive
        });

        if (result.IsFailure)
        {
            user.IsActive = !user.IsActive;
            await AlertService.JSErrorAsync(L.GetLangValue("lbl_update_error"));
            return;
        }

        await AlertService.JSSuccessAsync(L.GetLangValue("lbl_update_success"));

        await LoadUsers();
    }
}
