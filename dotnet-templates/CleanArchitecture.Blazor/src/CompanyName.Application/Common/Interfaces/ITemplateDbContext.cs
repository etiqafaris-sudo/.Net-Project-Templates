using CompanyName.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CompanyName.Application.Common.Interfaces;

public interface ITemplateDbContext
{
    IDbConnection GetDbConnection();
    Task<IDbConnection> GetOpenDbConnectionAsync(CancellationToken cancellationToken = default);

    public DbSet<GlUser> GlUsers { get; set; }
    public DbSet<GlParametersDescription> GlParametersDescriptions { get; set; }
    public DbSet<GlParameter> GlParameters { get; set; }
    public DbSet<GlUserGroup> GlUserGroups { get; set; }
    public DbSet<GlPortalMenu> GlPortalMenus { get; set; }
    public DbSet<GlUserGroupMenu> GlUserGroupMenus { get; set; }
    public DbSet<GLAtatchment> GlAttachments { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default!);
}
