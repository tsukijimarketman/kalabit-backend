using Data.Configurations;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public sealed class NotificationDbContext(
    DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public const string Schema = "kalabit";
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<NotificationIdentity> NotificationIdentities => Set<NotificationIdentity>(); 
    public DbSet<Notification> Notifications => Set<Notification>(); 
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>(); 
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>(); 
    public DbSet<NotificationAttempt> NotificationAttempts => Set<NotificationAttempt>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<ClientDailyUsage> ClientDailyUsages => Set<ClientDailyUsage>();
    public DbSet<KeepAliveHeartbeat> KeepAliveHeartbeats => Set<KeepAliveHeartbeat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NotificationDbContext).Assembly);
    }
}
