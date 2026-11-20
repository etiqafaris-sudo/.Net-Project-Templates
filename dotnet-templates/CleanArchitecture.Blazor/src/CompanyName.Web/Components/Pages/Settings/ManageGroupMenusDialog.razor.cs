using CompanyName.Application.Features.Menus.Commands.AssignGroupMenu;
using CompanyName.Application.Features.Menus.Queries.GetAllMenus;
using CompanyName.Application.Features.Menus.Queries.GetAllUserGroupMenu;
using CompanyName.Web.Models.Settings;
using CompanyName.Web.Models.Users;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class ManageGroupMenusDialog : AppComponentBase<GlUserGroupMenuDto>
{
    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Parameter]
    public GlUserGroupDto? Group { get; set; }

    protected IReadOnlyList<GLPortalMenuDto>? allMenus;
    protected List<int> selectedMenuIds = new();

    protected override async Task OnInitializedAsync()
    {
        allMenus = Array.Empty<GLPortalMenuDto>();
        if (Group == null)
        {
            DialogService.Close(false);
            return;
        }

        var menusResult = await Mediator.Send(new GetAllMenusQuery());
        if (menusResult.IsFailure)
        {
            await AlertService.JSErrorAsync($"Failed to load menus: {menusResult.Error}");
            DialogService.Close(false);
            return;
        }

        var flatMenus = menusResult.Value
            .Select(x => new GLPortalMenuDto
            {
                Id = x.Id,
                Icon = x.Icon,
                IsActive = x.IsActive ?? 0,
                MenuName = x.MenuName,
                MenuUrl = x.MenuUrl,
                OrderNo = x.OrderNo,
                ParentId = x.ParentId
            })
            .ToList();

        allMenus = BuildHierarchy(flatMenus) ?? Array.Empty<GLPortalMenuDto>();

        var assignmentsResult = await Mediator.Send(new GetAllUserGroupMenuQuery(Group.Id));
        if (assignmentsResult.IsFailure)
        {
            await AlertService.JSErrorAsync($"Failed to load menu assignments: {assignmentsResult.Error}");
            DialogService.Close(false);
            return;
        }

        selectedMenuIds = assignmentsResult.Value
            .Where(a => a.MenuId.HasValue && a.MenuId.Value > 0)
            .Select(a => a.MenuId!.Value)
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
            var result = await Mediator.Send(new AssignGroupMenuCommand(Group.Id, selectedMenuIds));

            if (result.IsFailure)
            {
                await AlertService.JSErrorAsync($"Failed to save menu assignments: {result.Error}");
                return;
            }

            await AlertService.JSSuccessAsync("Menu assignments updated successfully");
            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            await AlertService.JSErrorAsync($"Failed to save menu assignments: {ex.Message}");
        }
    }

    private static IReadOnlyList<GLPortalMenuDto>? BuildHierarchy(IReadOnlyList<GLPortalMenuDto>? flatList)
    {
        if (flatList == null)
        {
            return null;
        }

        var result = new List<GLPortalMenuDto>();

        var mains = flatList
            .Where(m => m.ParentId == null || m.ParentId == 0)
            .OrderBy(m => m.OrderNo)
            .ToList();

        foreach (var main in mains)
        {
            result.Add(main);
            var children = flatList
                .Where(m => m.ParentId == main.Id)
                .OrderBy(m => m.OrderNo)
                .ToList();
            result.AddRange(children);
        }

        var processedIds = result.Select(r => r.Id).ToHashSet();
        var orphans = flatList.Where(m => !processedIds.Contains(m.Id)).ToList();
        result.AddRange(orphans);

        return result;
    }
}
