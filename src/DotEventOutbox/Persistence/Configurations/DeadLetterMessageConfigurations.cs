using DotEventOutbox.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotEventOutbox.Persistence.Configurations;
internal sealed class DeadLetterMessageConfigurations : IEntityTypeConfiguration<DeadLetterMessage>
{
    public void Configure(EntityTypeBuilder<DeadLetterMessage> builder)
    {
        builder.ToTable("DeadLetterMessages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.OccurredOnUtc).IsRequired();
        builder.Property(e => e.Error).IsRequired();
        builder.Property(e => e.RetryCount).IsRequired();
        builder.Property(e => e.LastErrorOccurredOnUtc).IsRequired();
    }
}