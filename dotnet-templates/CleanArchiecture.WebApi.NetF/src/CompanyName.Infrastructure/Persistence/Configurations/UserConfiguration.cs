using System.Data.Entity.ModelConfiguration;
using CompanyName.Domain.Entities;

namespace CompanyName.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : EntityTypeConfiguration<User>
{
    public UserConfiguration()
    {
        ToTable("users");

        HasKey(x => x.Id);

        Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        HasIndex(x => x.Email)
            .IsUnique();

        Property(x => x.FullName)
            .HasMaxLength(256)
            .IsRequired();

        Property(x => x.Status)
            .IsRequired();

        Property(x => x.CreatedAtUtc)
            .IsRequired();

        Property(x => x.RowVersion)
            .IsRowVersion();
    }
}