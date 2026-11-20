using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.__Module__.Persistence.Messaging;

/// <summary>The outbox table, in the module's own schema. Generated; keep in step with the template.</summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.MessageId);
        builder.Property(m => m.MessageId).ValueGeneratedNever();
        builder.Property(m => m.Type).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(2000);
        builder.HasIndex(m => new { m.NextAttemptAt, m.OccurredAt })
            .HasFilter("[ProcessedAt] IS NULL AND [DeadLetteredAt] IS NULL");
    }
}
