using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations;

public sealed class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        builder.ToTable("notification_recipients");

        builder.HasKey(recipient => recipient.Id);

        builder.Property(recipient => recipient.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(recipient => recipient.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(recipient => recipient.NotificationId)
            .HasColumnName("notification_id")
            .IsRequired();

        builder.Property(recipient => recipient.NotificationCreatedAt)
            .HasColumnName("notification_created_at")
            .IsRequired();

        builder.Property(recipient => recipient.RecipientHash)
            .HasColumnName("recipient_hash")
            .HasColumnType("bytea")
            .IsRequired();

        builder.Property(recipient => recipient.MaskedAddress)
            .HasColumnName("masked_address")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(recipient => recipient.EncryptedAddressEnvelope)
            .HasColumnName("encrypted_address_envelope")
            .HasColumnType("bytea");

        builder.HasOne(recipient => recipient.Notification)
            .WithMany(notification => notification.Recipients)
            .HasForeignKey(recipient => new
            {
                recipient.NotificationId,
                recipient.NotificationCreatedAt
            })
            .HasPrincipalKey(notification => new
            {
                notification.Id,
                notification.CreatedAt
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(recipient => recipient.RecipientHash)
            .HasDatabaseName("ix_notification_recipients_recipient_hash");
    }
}
