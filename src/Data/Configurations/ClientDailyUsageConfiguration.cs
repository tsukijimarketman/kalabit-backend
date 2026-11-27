
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public sealed class ClientDailyUsageConfiguration : IEntityTypeConfiguration<ClientDailyUsage>
{
    public void Configure(EntityTypeBuilder<ClientDailyUsage> builder)
    {
        builder.ToTable("client_daily_usage", table =>
           table.HasCheckConstraint(
               "ck_client_daily_usage_count_nonnegative",
               "\"count\" >= 0"));

        builder.HasKey(usage => new { usage.ClientId, usage.UsageDate });

        builder.Property(usage => usage.ClientId)
            .HasColumnName("client_id");

        builder.Property(usage => usage.UsageDate)
            .HasColumnName("usage_date");

        builder.Property(usage => usage.Count)
            .HasColumnName("count")
            .IsRequired();

        builder.HasOne(usage => usage.Client)
            .WithMany()
            .HasForeignKey(usage => usage.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
