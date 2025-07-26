using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Trace;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Data;
using ProjectAthena.DbWorkerService.Seeders;

namespace ProjectAthena.DbWorkerService;

public class Worker(
    ILogger<Worker> logger,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    IConfiguration configuration) : BackgroundService
{
    public const string ActivitySourceName = "ProjectAthena.Migrations";

    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    // Database deletion control
    private const bool DELETE_DATABASE_ON_STARTUP = false; // Set to true to clean start

    private bool ShouldDeleteDatabase =>
        configuration.GetValue<bool>("Migration:DeleteDatabaseOnStartup", DELETE_DATABASE_ON_STARTUP);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating ProjectAthena database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Starting database migration and seeding process...");

            // Track if database was deleted
            bool databaseWasDeleted = false;

            // Delete database first (if enabled)
            if (ShouldDeleteDatabase)
            {
                databaseWasDeleted = await DeleteDatabaseAsync(dbContext, cancellationToken);
            }
            else
            {
                logger.LogInformation("Database deletion is disabled - keeping existing database");
            }

            await RunMigrationAsync(dbContext, cancellationToken);

            // Always attempt seeding (methods check if data already exists)
            await SeedDataAsync(scope.ServiceProvider, cancellationToken);

            logger.LogInformation("Database migration and seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database migration and seeding");
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private async Task<bool> DeleteDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogWarning("🔥 DELETING EXISTING DATABASE...");

        if (await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            logger.LogWarning("Database exists, deleting it...");
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            logger.LogWarning("✅ Database deleted successfully");
            return true;
        }
        else
        {
            logger.LogInformation("Database does not exist, skipping deletion");
            return false;
        }
    }

    private async Task RunMigrationAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("🚀 Starting database migration...");

        // Check if database exists
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        logger.LogInformation("Database connection status: {CanConnect}", canConnect);

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        var pendingMigrationsList = pendingMigrations.ToList();

        if (pendingMigrationsList.Count == 0)
        {
            logger.LogInformation("No pending migrations found");
        }
        else
        {
            logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                pendingMigrationsList.Count,
                string.Join(", ", pendingMigrationsList));
        }

        // Run migrations
        await dbContext.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("✅ Database migration completed successfully");

        // Log applied migrations
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        logger.LogInformation("Total applied migrations: {Count}", appliedMigrations.Count());
    }

    private async Task SeedDataAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        logger.LogInformation("🌱 Starting comprehensive data seeding...");

        var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure roles exist first
        await EnsureRolesAsync(scopedProvider, cancellationToken);

        // Get seeding configuration
        var seedingConfig = configuration.GetSection("DatabaseSeeding").Get<SeedingConfiguration>() ?? new SeedingConfiguration();

        // Use new seeding system
        var masterSeederLogger = scopedProvider.GetRequiredService<ILogger<MasterSeeder>>();
        var masterSeeder = new MasterSeeder(dbContext, userManager, seedingConfig, masterSeederLogger, scopedProvider);
        await masterSeeder.SeedAllAsync();

        logger.LogInformation("✅ Comprehensive data seeding completed successfully");
    }

    private async Task EnsureRolesAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Teacher", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }


}
