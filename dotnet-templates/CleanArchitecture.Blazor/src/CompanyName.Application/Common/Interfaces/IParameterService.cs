using System.Collections.Generic;
using System.Threading.Tasks;
using CompanyName.Domain.Entities;

namespace CompanyName.Application.Common.Interfaces;

public interface IParameterService
{
    Task<IEnumerable<GlParametersDescription>> GetAllParameterDescriptionsAsync();
    
    Task<IEnumerable<GlParameter>> GetParametersByTypeIdAsync(int parameterTypeId);

    Task<int> SaveParameterGroupAsync(GlParametersDescription master, List<GlParameter> details);
    
    Task<bool> IsParameterIdExistAsync(string parameterId, int currentTypeId = 0);
    
    Task<int> DeleteParameterGroupAsync(int parameterTypeId);
    
    Task<int> UpdateParameterStatusAsync(GlParametersDescription group);
}
