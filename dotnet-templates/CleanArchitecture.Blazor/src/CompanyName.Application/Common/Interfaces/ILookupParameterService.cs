using CompanyName.Application.Common.Models;

namespace CompanyName.Application.Common.Interfaces
{
    public interface ILookupParameterService
    {
        ValueTask<IReadOnlyList<Gl_ParameterDescriptionResponse>> GetAllParametersAsync(CancellationToken cancellationToken = default);
        ValueTask<IReadOnlyList<DropdownItem>> GetParametersAsync(string parameterName, string culture = "my", CancellationToken cancellationToken = default);
    }
}
