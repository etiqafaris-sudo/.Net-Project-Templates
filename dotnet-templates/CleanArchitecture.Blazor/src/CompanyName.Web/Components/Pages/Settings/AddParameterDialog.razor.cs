using CompanyName.Application.Features.Parameters.Commands.CreateParameterDesc;
using CompanyName.Application.Features.Parameters.Queries.GetChildMenu;
using CompanyName.Web.Models.Settings;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class AddParameterDialog : AppComponentBase<GlParameterDescDto>
{

    [Parameter]
    public GlParameterDescDto? MasterParameter { get; set; }

    protected bool IsEditMode => MasterParameter is not null;

    [Inject]
    public IMediator Mediator { get; set; } = default!;

    private List<GlParameterDto> childParams = new();
    private RadzenDataGrid<GlParameterDto>? childGrid;

    protected override async Task OnInitializedAsync()
    {
        if (MasterParameter is not null)
        {
            Model.ParameterTypeId = MasterParameter.ParameterTypeId;
            Model.ParameterDescription = MasterParameter.ParameterDescription;
            Model.Sort = MasterParameter.Sort;
            Model.IsActive = MasterParameter.IsActive;

            var result = await Mediator.Send(new GetChildMenuQuery(MasterParameter.ParameterTypeId));
            if (result.IsSuccess)
            {
                childParams = result.Value.Select(c => new GlParameterDto
                {
                    GlParameterId = c.GlParameterId,
                    ParameterId = c.ParameterId,
                    ParameterCode = c.ParameterCode,
                    ParameterDescription = c.ParameterDescription,
                    ParameterDescriptionEn = c.ParameterDescriptionEn,
                    Sort = c.Sort,
                    ParameterTypeId = c.ParameterTypeId
                }).ToList();
            }
        }
    }

    private async Task AddChildParameter()
    {
        var result = await DialogService.OpenAsync<AddChildParameterDialog>(L.GetLangValue("lbl_btn_add_child"),
               new Dictionary<string, object?>(),
               new DialogOptions() { Width = "600px", Height = "auto", Resizable = false, Draggable = false });

        if (result != null)
        {
            var newChild = result as GlParameterDto;
            if (newChild != null)
            {
                childParams.Add(newChild);
                if (childGrid != null)
                {
                    await childGrid.Reload();
                }
            }
        }
    }

    private async Task RemoveChildParameter(GlParameterDto child)
    {
        childParams.Remove(child);
        if (childGrid != null)
        {
            await childGrid.Reload();
        }
    }

    private async Task Submit()
    {
        var master = Model;
        if (childParams.Any(c => string.IsNullOrWhiteSpace(c.ParameterId)))
        {
            await JSWarningAsync("Child Parameter ID cannot be empty");
            return;
        }

        var duplicateChildId = childParams
            .GroupBy(c => c.ParameterId)
            .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key))
            .Select(g => g.Key)
            .FirstOrDefault();

        if (duplicateChildId != null)
        {
            await JSWarningAsync($"Duplicate Child Parameter ID: {duplicateChildId}");
            return;
        }

        var result = await Mediator.Send(new CreateParameterDescCommand
        {
            GLParameterDesc = new GLParameterDescRequest(
                master.ParameterTypeId,
                master.ParameterDescription,
                master.Sort),
            Children = childParams.Select(c => new GLParameterRequest(
                c.GlParameterId,
                c.ParameterId,
                c.ParameterId,
                c.ParameterTypeId,
                c.ParameterDescription,
                c.ParameterDescriptionEn,
                c.Sort)).ToList()
        });

        if (result.IsFailure)
        {
            var error = result.Error ?? string.Empty;
            if (error is "Parameter ID Exist" or "Parameter master not found.")
            {
                await JSWarningAsync(error);
                return;
            }

            await JSErrorAsync(master.ParameterTypeId == 0
                ? L.GetLangValue("lbl_create_error")
                : L.GetLangValue("lbl_update_error"));
            return;
        }

        await JSSuccessAsync(master.ParameterTypeId == 0
            ? L.GetLangValue("lbl_create_success")
            : L.GetLangValue("lbl_update_success"));
        DialogService.Close(true);
    }

    private void Cancel()
    {
        DialogService.Close(false);
    }
}
