using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Configurations;

public sealed class TemplateVersionConfiguration : IEntityTypeConfiguration<TemplateVersion>
{
    public void Configure(EntityTypeBuilder<TemplateVersion> builder)
    {
        builder.ToTable("template_versions", table =>
        {
            table.HasCheckConstraint(
                "ck_template_versions_version_number_positive",
                "\"version_number\" > 0");
        });

        builder.HasKey(version => version.Id);

        builder.Property(version => version.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(version => version.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(version => version.TemplateId)
            .HasColumnName("template_id")
            .IsRequired();

        builder.Property(version => version.VersionNumber)
            .HasColumnName("version_number")
            .IsRequired();

        builder.Property(version => version.SubjectTemplate)
            .HasColumnName("subject_template")
            .IsRequired();

        builder.Property(version => version.HtmlBodyTemplate)
            .HasColumnName("html_body_template")
            .IsRequired();

        builder.Property(version => version.TextBodyTemplate)
            .HasColumnName("text_body_template")
            .IsRequired();

        builder.Property(version => version.VariablesSchemaJson)
            .HasColumnName("variables_schema")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(version => version.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(version => version.ContentHash)
            .HasColumnName("content_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(version => new { version.TemplateId, version.VersionNumber })
            .IsUnique()
            .HasDatabaseName("ux_template_versions_template_id_version_number");
    }
}
