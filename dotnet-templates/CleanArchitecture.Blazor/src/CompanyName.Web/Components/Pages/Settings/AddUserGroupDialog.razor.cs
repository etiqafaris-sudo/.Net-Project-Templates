using CompanyName.Application.Features.Users.Commands.CreateUserGroup;
using CompanyName.Application.Features.Users.Commands.UpdateUserGroup;
using CompanyName.Domain.Entities;
using CompanyName.Web.Models.Users;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class AddUserGroupDialog : AppComponentBase<GlUserGroupDto>
{
    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Parameter]
    public GlUserGroupDto? Group { get; set; }

    public bool IsActiveBool
    {
        get => Model.IsActive == 1;
        set => Model.IsActive = value ? 1 : 0;
    }

    protected override void OnInitialized()
    {
        if (Group != null)
        {
            Model = new GlUserGroupDto
            {
                Id = Group.Id,
                GroupCode = Group.GroupCode,
                GroupName = Group.GroupName,
                IsActive = Group.IsActive
            };
        }
        else
        {
            Model.Id = 0;
            // Default active
            Model.IsActive = 1;
        }
    }

    public async Task OnSubmit()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Model.GroupName))
            {
                await AlertService.JSWarningAsync("Group Name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.GroupCode))
            {
                await AlertService.JSWarningAsync("Group Code is required.");
                return;
            }


            if (Model.Id == 0)
            {
                var result = await Mediator.Send(new CreateUserGroupCommand(Model.GroupCode, Model.GroupName, Model.IsActive));

                if (result.IsFailure)
                {
                    await AlertService.JSErrorAsync($"Failed to save group: {result.Error}");
                    return;
                }

                await AlertService.JSSuccessAsync("Group created successfully");
            }
            else
            {
                var result = await Mediator.Send(new UpdateUserGroupCommand(Model.Id, Model.GroupCode, Model.GroupName, Model.IsActive));

                if (result.IsFailure)
                {
                    await AlertService.JSErrorAsync($"Failed to update group: {result.Error}");
                    return;
                }

                await AlertService.JSSuccessAsync("Group updated successfully");
            }

            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            await AlertService.JSErrorAsync($"Failed to save group: {ex.Message}");
        }
    }
}
