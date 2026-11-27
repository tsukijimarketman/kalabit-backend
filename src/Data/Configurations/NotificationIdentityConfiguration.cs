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
        builder.ToTable("notification_identities");

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
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(identity => identity.PayloadHash)
            .HasColumnName("payload_hash")
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(identity => identity.IdempotencyExpiresAt)
            .HasColumnName("idempotency_expires_at")
            .IsRequired();

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
        .HasDatabaseName("ux_notification_identities_client_idempotency_key");

        builder.HasAlternateKey(identity => new
        {
            identity.Id,
            identity.CreatedAt
        })
        .HasName("ak_notification_identities_id_created_at");
    }
}
