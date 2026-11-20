using CompanyName.Application.Common.Models;
using CompanyName.Application.Features.Users.Commands.CreateNewUser;
using CompanyName.Application.Features.Users.Commands.UpdateUserProfile;
using CompanyName.Application.Features.Users.Queries.GetUserGroup;
using CompanyName.Web.Models.Users;
using Mediator;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class AddUserDialog : AppComponentBase<UserDto>
{
    [Inject]
    private IMediator _mediator { get; set; } = default!;

    [Inject]
    private ILogger<AddUserDialog> _logger { get; set; }

    [Parameter]
    public UserDto? User { get; set; }

    private bool IsEditMode => User is not null;

    List<DropdownItem> roles = new();

    protected override async Task OnInitializedAsync()
    {
        _logger.LogInformation("Initializing AddUserDialog component. {0}", JsonSerializer.Serialize(User));
        // If a user was passed in, this is EDIT mode. We clone it to avoid modifying the grid directly
        if (User != null)
        {
            Model.Id = User.Id;
            Model.Email = User.Email;
            Model.Fullname = User.Fullname;
            Model.Role = User.Role;
        }

        await LoadUserGroupOptionsAsync();

        if (User == null)
        {
            Model.Role = string.Empty;
        }
    }

    private async Task LoadUserGroupOptionsAsync()
    {
        var selectPrompt = string.Equals(L.CurrentCulture, "en", StringComparison.OrdinalIgnoreCase)
            ? "-- Please Select --"
            : "-- Sila Pilih --";

        roles =
        [
            new DropdownItem { Text = selectPrompt, Value = string.Empty }
        ];

        var result = await _mediator.Send(new GetUserGroupQuery());
        if (result.IsSuccess)
        {
            foreach (var group in result.Value.Where(g => g.IsActive == 1))
            {
                roles.Add(new DropdownItem
                {
                    Value = group.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(group.GroupName) ? group.GroupCode ?? group.Id.ToString() : group.GroupName
                });
            }
        }
    }

    public void Dispose()
    {
        L.OnChange -= () => InvokeAsync(StateHasChanged);
    }

    async Task Submit(UserDto args)
    {
        if (args.Id == 0) //new user
        {
            var result = await _mediator.Send(new CreateNewUserCommand()
            {
                Email = args.Email,
                Password = args.Password,
                Fullname = args.Fullname,
                Role = args.Role,
            });

            if (result.IsSuccess)
            {
                await AlertService.JSSuccessAsync(L.GetLangValue("lbl_create_success"));
            }
            else
            {
                await AlertService.JSErrorAsync(result.Error ?? L.GetLangValue("lbl_create_error"));
                return;
            }
        }
        else
        {
            var result = await _mediator.Send(new UpdateUserProfileCommand()
            {
                Id = args.Id,
                Email = args.Email,
                Fullname = args.Fullname,
                Role = args.Role
            });

            if (result.IsSuccess)
            {
                await AlertService.JSSuccessAsync(L.GetLangValue("lbl_update_success"));
            }
            else
            {
                await AlertService.JSErrorAsync(result.Error ?? L.GetLangValue("lbl_update_error"));
                return;
            }
        }

        DialogService.Close(true);
    }

    void Cancel()
    {
        DialogService.Close(false);
    }
}
