using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;

namespace CompanyName.Infrastructure.Persistence.Seed;

public static class ApplicationDataSeeder
{
    public const string DefaultAdminEmail = "admin@company.com";

    public const string DefaultAdminPasswordPlain = "Admin@123";

    public static void SeedGlUsers(TemplateDbContext context, IEncryptionService encryption)
    {
        var defaultGroup = EnsureDefaultUserGroup(context);

        var adminExists = context.GlUsers.Any(u =>
            u.Email == DefaultAdminEmail && u.IsDeleted == 0);

        if (adminExists)
        {
            return;
        }

        context.GlUsers.Add(new GlUser
        {
            Email = DefaultAdminEmail,
            Password = encryption.Encrypt(DefaultAdminPasswordPlain),
            Fullname = "System Administrator",
            Role = defaultGroup.Id.ToString(),
            IsActive = 1,
            IsDeleted = 0,
            AudAddDate = DateTime.Now,
        });

        context.SaveChanges();
    }

    private static GlUserGroup EnsureDefaultUserGroup(TemplateDbContext context)
    {
        var existingGroup = context.GlUserGroups
            .Where(g => g.IsDeleted == 0)
            .OrderBy(g => g.Id)
            .FirstOrDefault();

        if (existingGroup is not null)
        {
            return existingGroup;
        }

        var defaultGroup = new GlUserGroup
        {
            GroupCode = "SA",
            GroupName = "SuperAdmin",
            IsActive = 1,
            IsDeleted = 0,
            AudAddDate = DateTime.Now,
        };

        context.GlUserGroups.Add(defaultGroup);
        context.SaveChanges();

        return defaultGroup;
    }
}
