using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Data.Interceptors;

namespace Data;

public sealed class NotificationDbContextFactory
    : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
        {
            directory = directory.Parent;
        }

        var basePath = directory?.FullName ?? Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("NotificationDatabase");

        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.EndsWith("Password=", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Configure ConnectionStrings:NotificationDatabase in appsettings.Development.json " +
                "or the ConnectionStrings__NotificationDatabase environment variable.");
        }

        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(connectionString, npgsql => 
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", NotificationDbContext.Schema))
            .AddInterceptors(new AuditTimestampsInterceptor(TimeProvider.System))
            .Options;

        return new NotificationDbContext(options);
    }
}
