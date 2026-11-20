using System.Data;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CompanyName.Infrastructure.Persistence;

public sealed class TemplateDbContext : DbContext, ITemplateDbContext
{
    public DbSet<GlUser> GlUsers { get; set; }
    public DbSet<GlParametersDescription> GlParametersDescriptions { get; set; }
    public DbSet<GlParameter> GlParameters { get; set; }
    public DbSet<GlUserGroup> GlUserGroups { get; set; }
    public DbSet<GlPortalMenu> GlPortalMenus { get; set; }
    public DbSet<GlUserGroupMenu> GlUserGroupMenus { get; set; }
    public DbSet<GLAtatchment> GlAttachments { get; set; }

    public TemplateDbContext(DbContextOptions<TemplateDbContext> options)
        : base(options)
    {
    }

    public IDbConnection GetDbConnection() => Database.GetDbConnection();

    public async Task<IDbConnection> GetOpenDbConnectionAsync(CancellationToken cancellationToken = default)
    {
        await Database.OpenConnectionAsync(cancellationToken);
        return GetDbConnection();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
