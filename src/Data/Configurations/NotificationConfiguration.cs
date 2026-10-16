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
        builder.ToTable("notifications");

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
            .HasDatabaseName("ix_notification_correlation_id");
    }
}
