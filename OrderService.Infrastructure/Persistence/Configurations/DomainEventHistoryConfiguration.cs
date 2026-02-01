using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.AggregateId)
            .IsRequired();

        builder.Property(e => e.EventData)
            .IsRequired();

        builder.Property(e => e.OccurredOn)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.IsProcessed)
            .IsRequired();

        builder.Property(e => e.ProcessedAt);

        builder.Property(e => e.RetryCount)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasIndex(e => new { e.IsProcessed, e.CreatedAt });
        builder.HasIndex(e => e.AggregateId);
    }
}
