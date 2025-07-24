using Bogus;
using Microsoft.AspNetCore.Identity;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;

namespace ProjectAthena.DbWorkerService.Seeders;

public class UserSeeder : ISeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SeedingConfiguration _config;
    private readonly ILogger<UserSeeder> _logger;

    public UserSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SeedingConfiguration config,
        ILogger<UserSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAdminsAsync();
        await SeedTeachersAsync();
        await SeedStudentsAsync();
    }

    private async Task SeedAdminsAsync()
    {
        if (_context.Users.Any(u => u.Role == UserRole.Admin))
        {
            _logger.LogInformation("Admins already exist, skipping seeding");
            return;
        }

        var adminFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLower())
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.EmailConfirmed, f => true)
            .RuleFor(u => u.Role, f => UserRole.Admin)
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime)
            .RuleFor(u => u.IsActive, f => true);

        var admins = adminFaker.Generate(_config.AdminCount);

        foreach (var admin in admins)
        {
            var result = await _userManager.CreateAsync(admin, _config.DefaultPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                _logger.LogInformation("Created admin: {Email}", admin.Email);
            }
            else
            {
                _logger.LogError("Failed to create admin {Email}: {Errors}", admin.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedTeachersAsync()
    {
        if (_context.Users.Any(u => u.Role == UserRole.Teacher))
        {
            _logger.LogInformation("Teachers already exist, skipping seeding");
            return;
        }

        var teacherFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLower())
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.EmailConfirmed, f => true)
            .RuleFor(u => u.Role, f => UserRole.Teacher)
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(2).UtcDateTime)
            .RuleFor(u => u.IsActive, f => true);

        var teachers = teacherFaker.Generate(_config.TeacherCount);

        foreach (var teacher in teachers)
        {
            var result = await _userManager.CreateAsync(teacher, _config.DefaultPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(teacher, "Teacher");
                _logger.LogInformation("Created teacher: {Email}", teacher.Email);
            }
            else
            {
                _logger.LogError("Failed to create teacher {Email}: {Errors}", teacher.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedStudentsAsync()
    {
        if (_context.Users.Any(u => u.Role == UserRole.Student))
        {
            _logger.LogInformation("Students already exist, skipping seeding");
            return;
        }

        var studentFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLower())
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.EmailConfirmed, f => true)
            .RuleFor(u => u.Role, f => UserRole.Student)
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(3).UtcDateTime)
            .RuleFor(u => u.IsActive, f => true);

        var students = studentFaker.Generate(_config.StudentCount);

        foreach (var student in students)
        {
            var result = await _userManager.CreateAsync(student, _config.DefaultPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(student, "Student");
                _logger.LogInformation("Created student: {Email}", student.Email);
            }
            else
            {
                _logger.LogError("Failed to create student {Email}: {Errors}", student.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}