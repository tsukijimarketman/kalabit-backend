using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations;

public sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("templates");

        builder.HasKey(template => template.Id);

        builder.Property(template => template.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(template => template.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(template => template.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200);

        builder.Property(template => template.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(template => template.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(200);

        builder.Property(template => template.RowVersion)
            .IsRowVersion();

        builder.Property(template => template.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(template => template.TemplateKey)
            .HasColumnName("template_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(template => new { template.ClientId, template.TemplateKey })
            .IsUnique()
            .HasDatabaseName("ux_templates_client_id_template_key");

        builder.Property(template => template.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasOne(template => template.Client)
            .WithMany()
            .HasForeignKey(template => template.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(template => template.Versions)
            .WithOne(version => version.Template)
            .HasForeignKey(version => version.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
