using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities.Testings;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Infrastructure.Persistence;

public partial class TestingDbContext : DbContext, ITestingDbContext
{
    public TestingDbContext(DbContextOptions<TestingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.ToTable("Tbl_User");

            entity.Property(e => e.AudAction)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AudAddDate).HasColumnType("datetime");
            entity.Property(e => e.AudModDate).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);

            // Tbl_User stores flags as SQL bit; domain model uses int? (0/1).
            entity.Property(e => e.IsDeleted)
                .HasConversion(
                    v => v.HasValue ? v.Value != 0 : (bool?)null,
                    v => v.HasValue ? (v.Value ? 1 : 0) : (int?)null);
            entity.Property(e => e.IsActive)
                .HasConversion(
                    v => v.HasValue ? v.Value != 0 : (bool?)null,
                    v => v.HasValue ? (v.Value ? 1 : 0) : (int?)null);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
