using System.Data.Entity.ModelConfiguration;
using CompanyName.Domain.Entities;

namespace CompanyName.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : EntityTypeConfiguration<OutboxMessage>
{
    public OutboxMessageConfiguration()
    {
        ToTable("outbox_messages");

        HasKey(x => x.Id);

        Property(x => x.Type)
            .HasMaxLength(512)
            .IsRequired();

        Property(x => x.Content)
            .IsRequired();

        Property(x => x.OccurredOnUtc)
            .IsRequired();

        Property(x => x.AttemptCount)
            .IsRequired();
    }
}