using System;
using System.Collections.Generic;
using System.Text;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients", table => {
            table.HasCheckConstraint(
                "ck_clients_requests_per_minute_positive",
                "\"requests_per_minute\" > 0");
            table.HasCheckConstraint(
                "ck_clients_daily_quota_nonnegative",
                "\"daily_quota\" >= 0");
        });

        builder.HasKey(client => client.Id);

        builder.Property(client => client.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(client => client.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(client => client.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200);

        builder.Property(client => client.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(client => client.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(200);

        builder.Property(client => client.RowVersion)
            .IsRowVersion();

        builder.Property(client => client.KeycloakClientId)
            .HasColumnName("keycloak_client_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(client => client.KeycloakClientId)
            .IsUnique()
            .HasDatabaseName("ux_clients_keycloak_client_id");

        builder.Property(client => client.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(client => client.RequestsPerMinute)
            .HasColumnName("requests_per_minute")
            .IsRequired();

        builder.Property(client => client.DailyQuota)
            .HasColumnName("daily_quota")
            .IsRequired();

        builder.Property(client => client.IsSynthetic)
            .HasColumnName("is_synthetic")
            .IsRequired();
    }
}
