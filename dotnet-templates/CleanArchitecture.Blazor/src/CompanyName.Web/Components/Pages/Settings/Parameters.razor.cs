using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Parameters.Commands.DeleteParameterDesc;
using CompanyName.Application.Features.Parameters.Commands.ToggleParameterDesc;
using CompanyName.Web.Models.Settings;
using Mediator;
using Microsoft.AspNetCore.Components; // Added for ChangeEventArgs
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class Parameters : AppComponentBase<GlParameterDescDto>
{
    [Inject]
    public ILookupParameterService LookupParameterService { get; set; } = default!;

    [Inject]
    public ILogger<Parameters> _logger { get; set; } = default!;

    [Inject]
    public IMediator Mediator { get; set; } = default!;
    private IReadOnlyList<GlParameterDescDto>? allParameters;
    private IReadOnlyList<GlParameterDescDto>? parameters;
    private string searchText = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        allParameters = Array.Empty<GlParameterDescDto>();
        parameters = Array.Empty<GlParameterDescDto>();
        await LoadParameters();
    }

    private async Task LoadParameters()
    {
        await RunWithLoadingAsync(async () =>
        {
            var result = await LookupParameterService.GetAllParametersAsync();
            allParameters = result.Select(p => new GlParameterDescDto
            {
                ParameterTypeId = p.ParameterTypeId,
                ParameterId = p.ParameterId,
                ParameterDescription = p.ParameterDescription,
                Sort = p.Sort,
                IsActive = p.IsActive
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
            parameters = allParameters;
            return;
        }

        var lowerSearch = searchText.ToLower();

        parameters = allParameters?.Where(p =>
            (p.ParameterId != null && p.ParameterId.ToLower().Contains(lowerSearch)) ||
            (p.ParameterDescription != null && p.ParameterDescription.ToLower().Contains(lowerSearch)) ||
            (p.ParameterTypeId.ToString().Contains(lowerSearch))
        ).ToList();
    }

    private async Task AddParameter()
    {
        var result = await DialogService.OpenAsync<AddParameterDialog>(L.GetLangValue("lbl_btn_add_parameter"),
               new Dictionary<string, object?>(),
               new DialogOptions() { Width = "800px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadParameters();
        }
    }

    private async Task EditParameter(GlParameterDescDto paramToEdit)
    {
        _logger.LogInformation("Editing parameter with ID: {ParameterTypeId}", paramToEdit.ParameterTypeId);
        var result = await DialogService.OpenAsync<AddParameterDialog>("Edit Parameter", // TODO translation
              new Dictionary<string, object?> { { "MasterParameter", paramToEdit } },
              new DialogOptions() { Width = "800px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadParameters();
        }
    }

    private async Task DeleteParameter(GlParameterDescDto paramToDelete)
    {
        var confirmResult = await DialogService.Confirm(
            L.GetLangValue("lbl_confirm_delete"),
            L.GetLangValue("lbl_confirm_delete_title"),
            new ConfirmOptions() { OkButtonText = "Ya / Yes", CancelButtonText = "Batal / Cancel" }
        );

        if (confirmResult == true)
        {
            var result = await Mediator.Send(new DeleteParameterDescCommand(paramToDelete.ParameterTypeId));
            if (result.IsFailure)
            {
                await AlertService.JSErrorAsync(L.GetLangValue("lbl_delete_error"));
                return;
            }

            await AlertService.JSSuccessAsync(L.GetLangValue("lbl_delete_success"));
            await LoadParameters();
        }
    }

    private async Task ToggleParameterStatus(GlParameterDescDto paramToToggle)
    {
        bool isCurrentlyActive = paramToToggle.IsActive == 1;
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
            var result = await Mediator.Send(new ToggleParameterDescCommand(paramToToggle.ParameterTypeId));
            if (result.IsFailure)
            {
                await AlertService.JSErrorAsync(L.GetLangValue("lbl_update_error"));
                return;
            }

            await AlertService.JSSuccessAsync(L.GetLangValue("lbl_update_success"));
            await LoadParameters();
        }
    }
}
