using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.__Module__.Persistence.Messaging;

/// <summary>The outbox and inbox tables, in the module's own schema. Generated; keep in step with the template.</summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(m => m.MessageId);
        builder.Property(m => m.MessageId).ValueGeneratedNever();
        builder.Property(m => m.Sequence).UseIdentityColumn();
        builder.Property(m => m.Type).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(2000);
        builder.HasIndex(m => new { m.NextAttemptAt, m.Sequence })
            .HasFilter("[ProcessedAt] IS NULL AND [DeadLetteredAt] IS NULL");
    }
}

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(m => new { m.MessageId, m.Subscriber });
        builder.Property(m => m.Subscriber).HasMaxLength(200);
    }
}
