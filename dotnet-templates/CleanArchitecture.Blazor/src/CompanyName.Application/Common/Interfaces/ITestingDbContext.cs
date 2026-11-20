using CompanyName.Domain.Entities.Testings;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Application.Common.Interfaces
{
    public interface ITestingDbContext
    {
        DbSet<TblUser> TblUsers { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
