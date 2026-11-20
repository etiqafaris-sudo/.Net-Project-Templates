using CompanyName.Application.Common.Models;

namespace CompanyName.Application.Common.Interfaces;

public interface IDropdownParameterService
{
    Task<IReadOnlyList<DropdownItem>> GetParametersAsync(string parameterName, string culture = "my", CancellationToken cancellationToken = default);
}
