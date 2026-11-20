using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;

namespace CompanyName.Infrastructure.Services;

internal sealed class DropdownParameterService : IDropdownParameterService
{
    private readonly ILookupParameterService _lookupParameterService;

    public DropdownParameterService(ILookupParameterService lookupParameterService)
    {
        _lookupParameterService = lookupParameterService;
    }

    public async Task<IReadOnlyList<DropdownItem>> GetParametersAsync(
        string parameterName,
        string culture = "my",
        CancellationToken cancellationToken = default)
    {
        return await _lookupParameterService.GetParametersAsync(parameterName, culture, cancellationToken);
    }
}
