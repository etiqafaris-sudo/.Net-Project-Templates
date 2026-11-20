using System.Collections.Generic;
using System.Threading.Tasks;
using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Domain.Enums;
using System.Linq;

namespace CompanyName.Application.Services;

public class ParameterService : BaseService, IParameterService
{
    public ParameterService(IDal dal) : base(dal)
    {
    }

    public async Task<IEnumerable<GlParametersDescription>> GetAllParameterDescriptionsAsync()
    {
        return await StandardGetListByExpressionAsync<GlParametersDescription>(p => p.IsDeleted == 0);
    }

    public async Task<IEnumerable<GlParameter>> GetParametersByTypeIdAsync(int parameterTypeId)
    {
        return await StandardGetListByExpressionAsync<GlParameter>(p => p.ParameterTypeId == parameterTypeId && p.IsDeleted == 0);
    }

    public async Task<bool> IsParameterIdExistAsync(string parameterId, int currentTypeId = 0)
    {
        var existing = await StandardGetDataByExpressionAsync<GlParametersDescription>(p => p.ParameterId == parameterId && p.ParameterTypeId != currentTypeId && p.IsDeleted == 0);
        return existing != null;
    }

    public async Task<int> SaveParameterGroupAsync(GlParametersDescription master, List<GlParameter> details)
    {
        await _dal.BeginTransAsync();
        try
        {
            // 1. Save or Update Master
            master.IsDeleted = 0;
            if (master.ParameterTypeId == 0)
            {
                master.IsActive = 1;
                await StandardCudAsync(master, OpTypeEnum.Create);
                await _dal.SaveChangesAsync(); // Generate ID for ParameterTypeId
                master.ParameterId = master.ParameterTypeId.ToString();
                await StandardCudAsync(master, OpTypeEnum.Update);
            }
            else
            {
                var existingMaster = await StandardGetDataByExpressionAsync<GlParametersDescription>(p => p.ParameterTypeId == master.ParameterTypeId);
                if (existingMaster != null)
                {
                    existingMaster.ParameterDescription = master.ParameterDescription;
                    existingMaster.Sort = master.Sort;
                    existingMaster.IsActive = master.IsActive;
                    existingMaster.IsDeleted = master.IsDeleted;
                    await StandardCudAsync(existingMaster, OpTypeEnum.Update);
                    await _dal.SaveChangesAsync(); // Optional depending on updates schema
                }
            }

            // Note: In EF Core, if parameterTypeId is an Identity column, after Create it should be populated.
            // If it's not identity, we assume the user provided it or we have logic to generate it.
            int newTypeId = master.ParameterTypeId;

            // 2. Save Children
            var existingChildren = await GetParametersByTypeIdAsync(newTypeId);
            var existingChildrenList = existingChildren.ToList();

            // Soft-Delete children that are no longer in the details list
            foreach (var existingChild in existingChildrenList)
            {
                if (!details.Any(d => d.ParameterId == existingChild.ParameterId))
                {
                    existingChild.IsDeleted = 1;
                    await StandardCudAsync(existingChild, OpTypeEnum.Update);
                }
            }

            // Create or Update children from the incoming details list
            foreach (var detail in details)
            {
                detail.ParameterTypeId = newTypeId;
                detail.IsDeleted = 0;
                
                var existingChild = existingChildrenList.FirstOrDefault(c => c.ParameterId == detail.ParameterId);
                if (existingChild != null)
                {
                    // Update existing
                    existingChild.ParameterDescription = detail.ParameterDescription;
                    existingChild.ParameterDescriptionEn = detail.ParameterDescriptionEn;
                    existingChild.Sort = detail.Sort;
                    existingChild.ParameterCode = detail.ParameterId; // Set parameter_code equal to parameter_id
                    existingChild.IsDeleted = 0;
                    await StandardCudAsync(existingChild, OpTypeEnum.Update);
                }
                else
                {
                    // Create new
                    detail.ParameterTypeId = master.ParameterTypeId;
                    detail.ParameterCode = detail.ParameterId; // Set parameter_code equal to parameter_id
                    await StandardCudAsync(detail, OpTypeEnum.Create);
                }
            }

            await _dal.CommitAsync();
            
            return newTypeId;
        }
        catch
        {
            await _dal.RollbackAsync();
            throw;
        }
    }

    public async Task<int> DeleteParameterGroupAsync(int parameterTypeId)
    {
        var group = await StandardGetDataByExpressionAsync<GlParametersDescription>(p => p.ParameterTypeId == parameterTypeId);
        if (group == null) return 0;

        group.IsDeleted = 1;
        
        await _dal.BeginTransAsync();
        try
        {
            await StandardCudAsync(group, OpTypeEnum.Update);
            await _dal.CommitAsync();
        }
        catch
        {
            await _dal.RollbackAsync();
            throw;
        }
        
        return parameterTypeId;
    }

    public async Task<int> UpdateParameterStatusAsync(GlParametersDescription group)
    {
        var existingGroup = await StandardGetDataByExpressionAsync<GlParametersDescription>(p => p.ParameterTypeId == group.ParameterTypeId);
        if (existingGroup == null) return 0;

        existingGroup.IsActive = group.IsActive;

        await _dal.BeginTransAsync();
        try
        {
            await StandardCudAsync(existingGroup, OpTypeEnum.Update);
            await _dal.CommitAsync();
        }
        catch
        {
            await _dal.RollbackAsync();
            throw;
        }
        
        return group.ParameterTypeId;
    }
}
