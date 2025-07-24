using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Trace;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Data;

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
    private const bool DELETE_DATABASE_ON_STARTUP = false; // Set to false to keep existing database

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
            activity?.RecordException(ex);
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
        logger.LogInformation("🌱 Starting data seeding...");

        var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure roles exist first
        await EnsureRolesAsync(scopedProvider, cancellationToken);

        // Seed sample users
        await SeedUsersAsync(scopedProvider, cancellationToken);

        // Seed sample weather data
        await SeedWeatherDataAsync(dbContext, cancellationToken);

        logger.LogInformation("✅ Data seeding completed successfully");
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

    private async Task SeedUsersAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Admin user
        var adminEmail = "admin@projectathena.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Administrator",
                UserName = adminEmail,
                Email = adminEmail,
                Role = UserRole.Admin,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Created admin user: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed Teacher user
        var teacherEmail = "teacher@projectathena.com";
        if (await userManager.FindByEmailAsync(teacherEmail) == null)
        {
            var teacherUser = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Teacher",
                UserName = teacherEmail,
                Email = teacherEmail,
                Role = UserRole.Teacher,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(teacherUser, "Teacher123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(teacherUser, "Teacher");
                logger.LogInformation("Created teacher user: {Email}", teacherEmail);
            }
            else
            {
                logger.LogError("Failed to create teacher user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed Student user
        var studentEmail = "student@projectathena.com";
        if (await userManager.FindByEmailAsync(studentEmail) == null)
        {
            var studentUser = new ApplicationUser
            {
                FirstName = "Jane",
                LastName = "Student",
                UserName = studentEmail,
                Email = studentEmail,
                Role = UserRole.Student,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(studentUser, "Student123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
                logger.LogInformation("Created student user: {Email}", studentEmail);
            }
            else
            {
                logger.LogError("Failed to create student user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedWeatherDataAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.WeatherForecasts.AnyAsync(cancellationToken))
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            var locations = new[]
            {
                "New York", "London", "Tokyo", "Sydney", "Paris", "Berlin", "Moscow", "Cairo", "Mumbai", "Beijing"
            };

            var forecasts = new List<WeatherForecast>();

            for (int i = 1; i <= 10; i++)
            {
                forecasts.Add(new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Length)],
                    Location = locations[Random.Shared.Next(locations.Length)],
                    Humidity = Random.Shared.Next(30, 100),
                    WindSpeed = Random.Shared.Next(0, 50),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            dbContext.WeatherForecasts.AddRange(forecasts);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded {Count} weather forecasts", forecasts.Count);
        }
    }
}
