using CompanyName.Application.Features.Samples.Commands.DeleteUserSample;
using CompanyName.Application.Features.Samples.Queries.DbfactoryQuerySample;
using CompanyName.Web.Services;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.SampleDemo;

public partial class Sample : AppComponentBase<DbFactorySampleQueryResponse>
{
    [Inject]
    private IMediator Mediator { get; set; } = default!;

    [Inject]
    private DialogService DialogService { get; set; } = default!;

    [Inject]
    private AlertService AlertService { get; set; } = default!;

    private IReadOnlyList<DbFactorySampleQueryResponse>? _users = Array.Empty<DbFactorySampleQueryResponse>();

    private int? _selectedUserIdForAttachments;

    protected override async Task OnInitializedAsync()
    {
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        await RunWithLoadingAsync(async () =>
        {
            await Task.Delay(2000); //simulate loading phase lama sikit hehe
            _users = await Mediator.Send(new DbFactorySampleQuery());
        }, messageKey: "lbl_loading");
    }

    private async Task OpenAddUserDialog()
    {
        var result = await DialogService.OpenAsync<AddSampleUserDialog>(
            L.GetLangValue("lbl_btn_add_user"),
            new Dictionary<string, object?>(),
            new DialogOptions { Width = "1000px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadUsersAsync();
        }
    }

    private async Task EditUser(DbFactorySampleQueryResponse user)
    {
        var result = await DialogService.OpenAsync<AddSampleUserDialog>(
            "Edit User",
            new Dictionary<string, object?> { { nameof(AddSampleUserDialog.User), user } },
            new DialogOptions { Width = "1000px", Height = "auto", Resizable = false, Draggable = false });

        if (result == true)
        {
            await LoadUsersAsync();
        }
    }

    private Task OnAttachmentsChanged() =>
        // Optional: refresh parent state after upload/delete (demo hook).
        Task.CompletedTask;

    private async Task DeleteUser(DbFactorySampleQueryResponse user)
    {
        var confirmResult = await DialogService.Confirm(
            L.GetLangValue("lbl_confirm_delete"),
            L.GetLangValue("lbl_confirm_delete_title"),
            new ConfirmOptions
            {
                OkButtonText = L.GetLangValue("lbl_yes"),
                CancelButtonText = L.GetLangValue("lbl_cancel")
            });

        if (confirmResult != true)
        {
            return;
        }

        var result = await RunWithLoadingAsync(async () =>
        {
            await Task.Delay(2000); //simulate loading phase lama sikit hehe
            return await Mediator.Send(new DeleteUserSampleCommand { Id = user.user_id });
        }, messageKey: "lbl_deleting");

        if (result.IsFailure)
        {
            await AlertService.JSErrorAsync(L.GetLangValue("lbl_delete_error"));
        }

        //alert control manual or by service pon boleh.
        await AlertService.JSSuccessAsync(L.GetLangValue("lbl_delete_success"));
        await JSSuccessAsync(L.GetLangValue("lbl_delete_success"));
        await LoadUsersAsync();
    }
}
