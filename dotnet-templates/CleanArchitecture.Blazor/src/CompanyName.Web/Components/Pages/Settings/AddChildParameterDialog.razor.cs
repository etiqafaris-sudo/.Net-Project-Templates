using CompanyName.Domain.Entities;
using CompanyName.Web.Models.Settings;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace CompanyName.Web.Components.Pages.Settings;

public partial class AddChildParameterDialog : AppComponentBase<GlParameterDto>
{
    [Inject]
    public DialogService DialogService { get; set; } = default!;

    private void Submit(GlParameterDto param)
    {
        DialogService.Close(param);
    }

    private void Cancel()
    {
        DialogService.Close(null);
    }
}
