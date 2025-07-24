using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;

namespace ProjectAthena.DbWorkerService.Seeders;

public class MasterSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SeedingConfiguration _config;
    private readonly ILogger<MasterSeeder> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MasterSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SeedingConfiguration config,
        ILogger<MasterSeeder> logger,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _userManager = userManager;
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SeedAllAsync()
    {
        _logger.LogInformation("Starting comprehensive database seeding...");

        try
        {
            // Seed in specific order due to dependencies
            await SeedUsersAsync();
            await SeedStudentsAsync();
            await SeedTeachersAsync();
            await SeedCoursesAsync();
            await SeedEnrollmentsAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        var userSeederLogger = _serviceProvider.GetRequiredService<ILogger<UserSeeder>>();
        var userSeeder = new UserSeeder(_context, _userManager, _config, userSeederLogger);
        await userSeeder.SeedAsync();
    }

    private async Task SeedStudentsAsync()
    {
        var studentSeederLogger = _serviceProvider.GetRequiredService<ILogger<StudentSeeder>>();
        var studentSeeder = new StudentSeeder(_context, _config, studentSeederLogger);
        await studentSeeder.SeedAsync();
    }

    private async Task SeedTeachersAsync()
    {
        var teacherSeederLogger = _serviceProvider.GetRequiredService<ILogger<TeacherSeeder>>();
        var teacherSeeder = new TeacherSeeder(_context, _config, teacherSeederLogger);
        await teacherSeeder.SeedAsync();
    }

    private async Task SeedCoursesAsync()
    {
        var courseSeederLogger = _serviceProvider.GetRequiredService<ILogger<CourseSeeder>>();
        var courseSeeder = new CourseSeeder(_context, _config, courseSeederLogger);
        await courseSeeder.SeedAsync();
    }

    private async Task SeedEnrollmentsAsync()
    {
        var enrollmentSeederLogger = _serviceProvider.GetRequiredService<ILogger<EnrollmentSeeder>>();
        var enrollmentSeeder = new EnrollmentSeeder(_context, _config, enrollmentSeederLogger);
        await enrollmentSeeder.SeedAsync();
    }
}