using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public sealed class KeepAliveHeartbeatConfiguration : IEntityTypeConfiguration<KeepAliveHeartbeat>
{
    public void Configure(EntityTypeBuilder<KeepAliveHeartbeat> builder)
    {
        builder.ToTable("keepalive_heartbeats");

        builder.HasKey(heartbeat => heartbeat.Id);

        builder.Property(heartbeat => heartbeat.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(heartbeat => heartbeat.PingedAt)
            .HasColumnName("pinged_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(heartbeat => heartbeat.Source)
            .HasColumnName("source")
            .HasMaxLength(50)
            .IsRequired();
    }
}
