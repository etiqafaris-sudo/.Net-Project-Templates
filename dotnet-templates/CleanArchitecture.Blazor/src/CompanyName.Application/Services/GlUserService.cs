using System.Threading.Tasks;
using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;

namespace CompanyName.Application.Services;

public class GlUserService : BaseService
{
    public GlUserService(IDal dal) : base(dal)
    {
    }

    public async Task<GlUser> GetGlUserByIdAsync(int id)
    {
        // Using StandardReadRecordAsync<TBl>
        return await StandardReadRecordAsync<GlUser>(false, new object[] { id });
    }

    public async Task<GlUser?> LoginAsync(string email, string password)
    {
        // In real app, password should be hashed. Here we compare plain text as per user request/sample context.
        return await StandardGetDataByExpressionAsync<GlUser>(u => u.Email == email && u.Password == password && u.IsActive == 1 && u.IsDeleted == 0);
    }

    public async Task<int> CreateUserAsync(GlUser user)
    {
        user.IsActive = 1;
        user.IsDeleted = 0;
        
        await _dal.BeginTransAsync();
        try
        {
            await StandardCudAsync(user, CompanyName.Domain.Enums.OpTypeEnum.Create);
            await _dal.CommitAsync();
        }
        catch
        {
            await _dal.RollbackAsync();
            throw;
        }

        return user.Id;
    }

    public async Task<IEnumerable<GlUser>> GetAllUsersAsync()
    {
        return await StandardGetListByExpressionAsync<GlUser>(u => u.IsDeleted == 0);
    }

    public async Task<int> UpdateUserAsync(GlUser user)
    {
        var existingUser = await GetGlUserByIdAsync(user.Id);
        if (existingUser == null) return 0;

        // Apply changes from the cloned UI object to the DB tracked object
        existingUser.Fullname = user.Fullname;
        existingUser.Email = user.Email;
        existingUser.Password = user.Password;
        existingUser.Role = user.Role;
        existingUser.IsActive = user.IsActive;

        await _dal.BeginTransAsync();
        try
        {
            await StandardCudAsync(existingUser, CompanyName.Domain.Enums.OpTypeEnum.Update);
            await _dal.CommitAsync();
        }
        catch
        {
            await _dal.RollbackAsync();
            throw;
        }

        return user.Id;
    }

    public async Task<int> DeleteUserAsync(int id)
    {
        var user = await GetGlUserByIdAsync(id);
        if (user == null) return 0;

        user.IsDeleted = 1;

        await _dal.BeginTransAsync();
        try
        {
            await StandardCudAsync(user, CompanyName.Domain.Enums.OpTypeEnum.Update);
            await _dal.CommitAsync();
        }
        catch
        {
            await _dal.RollbackAsync();
            throw;
        }

        return user.Id;
    }
}
