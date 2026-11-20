using System;
using System.Threading.Tasks;
using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Domain.Enums;

namespace CompanyName.Application.Services;

public class TestTransactionService : BaseService
{
    public TestTransactionService(IDal dal) : base(dal)
    {
    }

    public async Task<string> PerformTestTransactionAsync()
    {
        try
        {
            await _dal.BeginTransAsync();

            var entity = new TestEntity
            {
                Name = "Transaction Test " + DateTime.Now.ToString()
            };

            // Using standard CUD
            await StandardCudAsync(entity, OpTypeEnum.Create);

            await _dal.CommitAsync();

            return "Transaction Successful! Entity Created.";
        }
        catch (Exception ex)
        {
            await _dal.RollbackAsync();
            return $"Transaction Failed: {ex.Message}";
        }
    }
}
