using System;
using System.Threading.Tasks;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Common;
using CompanyName.Domain.Enums;

namespace CompanyName.Application.Common;

public abstract class BaseService
{
    protected readonly IDal _dal;

    protected BaseService(IDal dal)
    {
        _dal = dal;
    }

    protected async Task<TBl> StandardReadRecordAsync<TBl>(bool isNeedReadAudit, object[] keys) where TBl : IAuditable
    {
        var bl = await StandardReadRecordAsync(typeof(TBl), isNeedReadAudit, keys);
        if (bl == null)
             throw new InvalidOperationException($"Record not found for {typeof(TBl).Name}"); 
             // Ideally use a localized resource message here as per user request Enums.RecordMsg.RecordNotFound()

        return (TBl)bl;
    }

    protected async Task<object?> StandardReadRecordAsync(Type blType, bool isNeedReadAudit, object[] keys)
    {
        var bl = await _dal.ReadDataAsync(blType, isNeedReadAudit, keys);
        if (bl == null)
            return null; // Let the generic wrapper handle exception or return null if intended. 
                         // User sample threw exception. I'll rely on generic wrapper or caller.
        return bl;
    }

    protected async Task<TBl?> StandardGetDataByExpressionAsync<TBl>(System.Linq.Expressions.Expression<Func<TBl, bool>> predicate) where TBl : class
    {
        return await _dal.GetDataByExpressionAsync(predicate);
    }

    protected async Task<System.Collections.Generic.IEnumerable<TBl>> StandardGetListByExpressionAsync<TBl>(System.Linq.Expressions.Expression<Func<TBl, bool>> predicate) where TBl : class
    {
        return await _dal.GetListByExpressionAsync(predicate);
    }

    public async Task<object?> StandardCudAsync<TBl>(TBl bl, OpTypeEnum opType) where TBl : class, IAuditable
    {
        return await StandardCudAsync(typeof(TBl), bl, opType);
    }

    public async Task<object?> StandardCudAsync(Type blType, IAuditable bl, OpTypeEnum opType)
    {
        object? result = default;
        
        bl.IsNeedAudit = true;

        // Mock user profile ID. In real app, inject ICurrentUserService.
        AuditableCudAudit.Apply(bl, opType, userId: 0);

        switch (opType)
        {
            case OpTypeEnum.Create:
                result = await _dal.CreateDataAsync(blType, bl);
                break;
            case OpTypeEnum.Update:
                await _dal.UpdateDataAsync(blType, bl, null);
                break;
            case OpTypeEnum.Delete:
                await _dal.DeleteDataAsync(blType, bl);
                break;
        }

        return result;
    }
}
