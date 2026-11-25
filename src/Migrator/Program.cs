using Data;
using Microsoft.EntityFrameworkCore;

try
{
    var factory = new NotificationDbContextFactory();
    await using var dbContext = factory.CreateDbContext(args);

    var pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

    if (pending.Count == 0)
    {
        Console.WriteLine("Database is up to date. Nothing to apply.");
        return 0;
    }

    Console.WriteLine($"Applying {pending.Count} migration(s).");
    foreach (var migration in pending)
    {
        Console.WriteLine($" - {migration}");
    }

    await dbContext.Database.MigrateAsync();

    var latest = (await dbContext.Database.GetAppliedMigrationsAsync()).LastOrDefault();
    Console.WriteLine($"Done. Database is now at: {latest}");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine("Migration FAILED:");
    Console.Error.WriteLine(exception);
    return 1;
}
