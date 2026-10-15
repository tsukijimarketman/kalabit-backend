using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations;

public sealed class NotificationIdentityConfiguration : IEntityTypeConfiguration<NotificationIdentity>
{
    public void Configure(EntityTypeBuilder<NotificationIdentity> builder)
    {
        builder.ToTable("notification_identities", table =>
        {
            table.HasCheckConstraint(
                "ck_notification_identities_idempotency_fields",
                """
                (
                    "idempotency_key" IS NULL
                    AND "payload_hash" IS NULL
                    AND "idempotency_expires_at" IS NULL
                )
                OR
                (
                    "idempotency_key" IS NOT NULL
                    AND "payload_hash" IS NOT NULL
                    AND "idempotency_expires_at" IS NOT NULL
                )
                """);
        });

        builder.HasKey(identity => identity.Id);

        builder.Property(identity => identity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(identity => identity.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(identity => identity.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(identity => identity.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(200);

        builder.Property(identity => identity.PayloadHash)
            .HasColumnName("payload_hash")
            .HasMaxLength(64);

        builder.Property(identity => identity.IdempotencyExpiresAt)
            .HasColumnName("idempotency_expires_at");

        builder.HasOne(identity => identity.Client)
            .WithMany()
            .HasForeignKey(identity => identity.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(identity => new
        {
            identity.ClientId,
            identity.IdempotencyKey
        })
        .IsUnique()
        .HasFilter("\"idempotency_key\" IS NOT NULL")
        .HasDatabaseName("ux_notification_identities_client_idempotency_key");
    }
}
