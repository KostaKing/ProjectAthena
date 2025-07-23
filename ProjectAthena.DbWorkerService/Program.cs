using Microsoft.AspNetCore.Identity;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using ProjectAthena.DbWorkerService;

var builder = Host.CreateApplicationBuilder(args);

// Add .NET Aspire service defaults
builder.AddServiceDefaults();

// Add the hosted service for migrations
builder.Services.AddHostedService<Worker>();

// Add OpenTelemetry tracing for the migration process
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

// Add the PostgreSQL DbContext using Aspire
builder.AddNpgsqlDbContext<ApplicationDbContext>("DefaultConnection");

// Add Identity services for seeding
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configure password options for seeding
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Configure lockout options
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configure user options
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var host = builder.Build();
host.Run();
