using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

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

        if (directory is null)
        {
            throw new InvalidOperationException(
                "Could not find appsettings.json in the current directory or its parent directories.");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.FullName)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString =
            configuration.GetConnectionString("NotificationDatabase");

        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.EndsWith("Password=", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Configure ConnectionStrings:NotificationDatabase in appsettings.Development.json.");
        }

        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NotificationDbContext(options);
    }
}
