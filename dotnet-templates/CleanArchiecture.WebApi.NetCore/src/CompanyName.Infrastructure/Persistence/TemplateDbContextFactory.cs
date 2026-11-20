using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CompanyName.Infrastructure.Persistence;

public sealed class TemplateDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
{
    public TemplateDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TemplateDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=TemplateDatabase;User Id=sa;Password=P@ssw0rd;Encrypt=True;TrustServerCertificate=True;Connect Timeout=30;Application Name=CompanyName.Api;");

        return new TemplateDbContext(optionsBuilder.Options);
    }
}