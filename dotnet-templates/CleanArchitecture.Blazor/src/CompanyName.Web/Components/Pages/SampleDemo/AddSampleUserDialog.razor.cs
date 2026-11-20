using CompanyName.Application.Features.Samples.Commands.CreateUser;
using CompanyName.Application.Features.Samples.Commands.UpdateUser;
using CompanyName.Application.Features.Samples.Queries.DbfactoryQuerySample;
using CompanyName.Web.Services;
using Mediator;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.SampleDemo;

public partial class AddSampleUserDialog
{
    [Inject]
    private IMediator Mediator { get; set; } = default!;

    [Inject]
    private AlertService AlertService { get; set; } = default!;

    [Parameter]
    public DbFactorySampleQueryResponse? User { get; set; }

    private readonly SampleUserFormModel model = new();

    protected override void OnParametersSet()
    {
        if (User is null)
        {
            model.Id = 0;
            model.Name = string.Empty;
            model.Email = string.Empty;
            return;
        }

        model.Id = User.user_id;
        model.Name = User.FullName;
        model.Email = User.Email;
    }

    private async Task OnSubmit(SampleUserFormModel _)
    {
        if (model.Id == 0)
        {
            await Mediator.Send(new CreateSampleUserCommand
            {
                Name = model.Name,
                Email = model.Email
            });
            await AlertService.JSSuccessAsync(L.GetLangValue("lbl_create_success"));
        }
        else
        {
            await Mediator.Send(new UpdateUserTestingCommand
            {
                Id = model.Id,
                Name = model.Name,
                Email = model.Email
            });
            await AlertService.JSSuccessAsync(L.GetLangValue("lbl_update_success"));
        }

        DialogService.Close(true);
    }

    private void Cancel() => DialogService.Close(false);

    public sealed class SampleUserFormModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
