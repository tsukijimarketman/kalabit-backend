using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations;

public sealed class NotificationAttemptConfiguration : IEntityTypeConfiguration<NotificationAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationAttempt> builder)
    {
        builder.ToTable("notification_attempts", table =>
        {
            table.HasCheckConstraint(
                "ck_notification_attempts_attempt_number_positive",
                "\"attempt_number\" > 0");

            table.HasCheckConstraint(
                "ck_notification_attempts_duration_nonnegative",
                "\"duration_milliseconds\" >= 0");
        });

        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(attempt => attempt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(attempt => attempt.NotificationId)
            .HasColumnName("notification_id")
            .IsRequired();

        builder.Property(attempt => attempt.NotificationCreatedAt)
            .HasColumnName("notification_created_at")
            .IsRequired();

        builder.Property(attempt => attempt.AttemptNumber)
            .HasColumnName("attempt_number")
            .IsRequired();

        builder.Property(attempt => attempt.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(attempt => attempt.CompletedAt)
            .HasColumnName("completed_at")
            .IsRequired();

        builder.Property(attempt => attempt.Outcome)
            .HasColumnName("outcome")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(attempt => attempt.SmtpStatusCode)
            .HasColumnName("smtp_status_code");

        builder.Property(attempt => attempt.WorkerId)
            .HasColumnName("worker_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(attempt => attempt.DurationMilliseconds)
            .HasColumnName("duration_milliseconds")
            .IsRequired();

        builder.HasOne(attempt => attempt.Notification)
            .WithMany(notification => notification.Attempts)
            .HasForeignKey(attempt => new
            {
                attempt.NotificationId,
                attempt.NotificationCreatedAt
            })
            .HasPrincipalKey(notification => new
            {
                notification.Id,
                notification.CreatedAt
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(attempt => new
        {
            attempt.NotificationId,
            attempt.NotificationCreatedAt,
            attempt.AttemptNumber
        })
        .IsUnique()
        .HasDatabaseName("ux_notification_attempts_notification_attempt_number");  
    }
}
