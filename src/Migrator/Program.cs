using Data;
using Microsoft.EntityFrameworkCore;

var factory = new NotificationDbContextFactory();
await using var dbContext = factory.CreateDbContext([]);

if (!await dbContext.Database.CanConnectAsync())
{
    throw new InvalidOperationException(
        "Could not connect to PostgreSQL using the configured connection string.");
}

Console.WriteLine("Connected to PostgreSQL. No migrations were applied.");
