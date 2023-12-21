using DotEventOutbox.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotEventOutbox.Infrastructure.EntityFramework.Configurations;
internal sealed class OutboxMessageConsumerConfigurations : IEntityTypeConfiguration<OutboxMessagesConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessagesConsumer> builder)
    {
        builder.ToTable("OutboxMessageConsumers");
        builder.HasKey(e => new { e.Id, e.Name });
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
    }
}