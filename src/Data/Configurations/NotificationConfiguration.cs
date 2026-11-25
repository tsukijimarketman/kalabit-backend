using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", table =>
        {
            table.HasCheckConstraint(
                "ck_notifications_attempt_count_nonnegative",
                "\"attempt_count\" >= 0");

            table.HasCheckConstraint(
                "ck_notifications_lease_fields",
                "(\"lease_owner\" IS NULL) = (\"lease_expires_at\" IS NULL)");
        });

        builder.HasKey(notification => new
        {
            notification.Id,
            notification.CreatedAt
        });

        builder.Property(notification => notification.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(notification => notification.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(notification => notification.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(notification => notification.TemplateVersionId)
            .HasColumnName("template_version_id")
            .IsRequired();

        builder.Property(notification => notification.CorrelationId)
            .HasColumnName("correlation_id")
            .IsRequired();

        builder.Property(notification => notification.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(notification => notification.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(notification => notification.AttemptCount)
            .HasColumnName("attempt_count")
            .IsRequired();

        builder.Property(notification => notification.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .IsRequired();

        builder.Property(notification => notification.LeaseOwner)
            .HasColumnName("lease_owner")
            .HasMaxLength(200);

        builder.Property(notification => notification.LeaseExpiresAt)
            .HasColumnName("lease_expires_at");

        builder.Property(notification => notification.LastFailureClass)
            .HasColumnName("last_failure_class")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(notification => notification.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(500);

        builder.Property(notification => notification.FinalizedAt)
            .HasColumnName("finalized_at");

        builder.Property(notification => notification.EncryptedPayloadEnvelope)
            .HasColumnName("encrypted_payload_envelope")
            .HasColumnType("bytea");

        builder.HasOne(notification => notification.Client)
            .WithMany()
            .HasForeignKey(notification => notification.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.TemplateVersion)
            .WithMany()
            .HasForeignKey(notification => notification.TemplateVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.Identity)
            .WithOne()
            .HasForeignKey<Notification>(notification => new
            {
                notification.Id,
                notification.CreatedAt
            })
            .HasPrincipalKey<NotificationIdentity>(identity => new
            {
                identity.Id,
                identity.CreatedAt
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(notification => new
        {
            notification.ClientId,
            notification.CreatedAt
        })
        .HasDatabaseName("ix_notifications_client_id_created_at");

        builder.HasIndex(notification => notification.CorrelationId)
            .HasDatabaseName("ix_notifications_correlation_id");

        builder.HasIndex(notification => notification.NextAttemptAt)
            .HasFilter("\"status\" IN ('Queued', 'Retrying')")
            .HasDatabaseName("ix_notifications_due");

        builder.HasIndex(notification => notification.LeaseExpiresAt)
            .HasFilter("\"status\" = 'Sending'")
            .HasDatabaseName("ix_notifications_expired_leases");
    }
}
