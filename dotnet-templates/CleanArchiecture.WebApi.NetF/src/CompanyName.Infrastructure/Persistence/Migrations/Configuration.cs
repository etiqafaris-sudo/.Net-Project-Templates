using System.Data.Entity.Migrations;
using CompanyName.Infrastructure.Persistence;

namespace CompanyName.Infrastructure.Persistence.Migrations;

public sealed class TemplateDbMigrationsConfiguration : DbMigrationsConfiguration<TemplateDbContext>
{
    public TemplateDbMigrationsConfiguration()
    {
        AutomaticMigrationsEnabled = false;
        MigrationsDirectory = @"Persistence\Migrations";
    }
}