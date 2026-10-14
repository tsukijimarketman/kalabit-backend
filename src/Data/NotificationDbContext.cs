using System;
using System.Collections.Generic;
using System.Text;
using Data.Configurations;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public sealed class NotificationDbContext(
    DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateVersion> TemplateVersion => Set<TemplateVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NotificationDbContext).Assembly);
    }
}
