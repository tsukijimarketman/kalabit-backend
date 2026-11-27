using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations;

public sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys", table =>
            table.HasCheckConstraint(
                "ck_api_keys_key_hash_length",
                "octet_length(\"key_hash\") = 32"));

        builder.HasKey(key => key.Id);

        builder.Property(key => key.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(key => key.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(key => key.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(key => key.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(key => key.Prefix)
            .HasColumnName("prefix")
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(key => key.Prefix)
            .IsUnique()
            .HasDatabaseName("ux_api_keys_prefix");

        builder.Property(key => key.KeyHash)
            .HasColumnName("key_hash")
            .HasColumnType("bytea")
            .IsRequired();

        builder.Property(key => key.Scopes)
            .HasColumnName("scopes")
            .IsRequired();

        builder.Property(key => key.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(key => key.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(key => key.LastUsedAt)
            .HasColumnName("last_used_at");

        builder.HasOne(key => key.Client)
            .WithMany(client => client.ApiKeys)
            .HasForeignKey(key => key.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(key => key.ClientId)
            .HasDatabaseName("ix_api_keys_client_id");
    }
}
