using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(message => message.Id);

            builder.Property(message => message.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(message => message.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(message => message.NotificationId)
                .HasColumnName("notification_id")
                .IsRequired();

            builder.Property(message => message.NotificationCreatedAt)
                .HasColumnName("notification_created_at")
                .IsRequired();

            builder.Property(message => message.Topic)
               .HasColumnName("topic")
               .HasMaxLength(200)
               .IsRequired();

            builder.Property(message => message.MessageKey)
                .HasColumnName("message_key")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(message => message.EnvelopeJson)
                .HasColumnName("envelope")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(message => message.HeadersJson)
                .HasColumnName("headers")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(message => message.PublishedAt)
                .HasColumnName("published_at");

            builder.HasOne(message => message.Notification)
                .WithMany()
                .HasForeignKey(message => new
                {
                    message.NotificationId,
                    message.NotificationCreatedAt
                })
                .HasPrincipalKey(notification => new
                {
                    notification.Id,
                    notification.CreatedAt
                })
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(message => message.CreatedAt)
                .HasFilter("\"published_at\" IS NULL")
                .HasDatabaseName("ix_outbox_messages_unpublished_created_at");
        }
    }
}
