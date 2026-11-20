using CompanyName.Application.Features.Users.Commands.AssignGroupUser;
using CompanyName.Application.Features.Users.Queries.ListAllActiveUser;
using CompanyName.Web.Models.Users;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class AssignGroupUsersDialog : AppComponentBase<UserDto>
{
    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Parameter]
    public GlUserGroupDto? Group { get; set; }

    protected IReadOnlyList<UserDto> allUsers = Array.Empty<UserDto>();
    protected IReadOnlyList<UserDto> filteredUsers = Array.Empty<UserDto>();
    protected List<int> selectedUserIds = new List<int>();
    private string searchText = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (Group == null)
        {
            DialogService.Close(false);
            return;
        }

        var result = await Mediator.Send(new ListAllActiveUserQuery());
        if (result.IsFailure)
        {
            await AlertService.JSErrorAsync($"Failed to load users: {result.Error}");
            DialogService.Close(false);
            return;
        }

        var activeUsers = result.Value;
        var groupIdStr = Group.Id.ToString();
        var existingAssignedUsers = activeUsers.Where(u => u.Role == groupIdStr).ToList();

        allUsers = activeUsers
            .Select(x => new UserDto
            {
                Id = x.Id,
                Fullname = x.Fullname,
                Email = x.Email
            })
            .ToList();

        selectedUserIds = existingAssignedUsers.Select(u => u.Id).ToList();
        ApplyFilter();
    }

    private void OnSearch(ChangeEventArgs args)
    {
        searchText = args.Value?.ToString() ?? string.Empty;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (allUsers.Count == 0)
        {
            filteredUsers = allUsers;
            return;
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            filteredUsers = allUsers;
            return;
        }

        var lowerSearch = searchText.ToLower();

        filteredUsers = allUsers
            .Where(u =>
                (u.Fullname != null && u.Fullname.ToLower().Contains(lowerSearch)) ||
                (u.Email != null && u.Email.ToLower().Contains(lowerSearch)))
            .ToList();
    }

    public async Task OnSubmit()
    {
        if (Group == null)
        {
            return;
        }

        try
        {
            var result = await Mediator.Send(new AssignGroupUserCommand(Group.Id, selectedUserIds));

            if (result.IsFailure)
            {
                await AlertService.JSErrorAsync($"Failed to save user assignments: {result.Error}");
                return;
            }

            await AlertService.JSSuccessAsync("User assignments updated successfully");
            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            await AlertService.JSErrorAsync($"Failed to save user assignments: {ex.Message}");
        }
    }
}
