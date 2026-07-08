using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectManagementSaaS.Infrastructure.Persistence;
using ProjectManagementSaaS.Infrastructure.Persistence.Seed;

namespace ProjectManagementSaaS.Api.Extensions;

public static partial class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitialization");
        var databaseOptions = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var seedOptions = services.GetRequiredService<IOptions<SeedDataOptions>>().Value;

        if (databaseOptions.ApplyMigrationsOnStartup)
        {
            LogApplyingMigrations(logger);
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
        }

        if (seedOptions.RunOnStartup)
        {
            LogRunningSeedData(logger);
            var seeder = services.GetRequiredService<IDatabaseSeeder>();
            await seeder.SeedAsync(app.Lifetime.ApplicationStopping);
        }
    }

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Applying EF Core migrations.")]
    private static partial void LogApplyingMigrations(ILogger logger);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Information,
        Message = "Running configured seed data.")]
    private static partial void LogRunningSeedData(ILogger logger);
}
