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

        // Create the specific admin user
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@projectathena.com",
            UserName = "admin@projectathena.com",
            EmailConfirmed = true,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

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

    private async Task SeedTeachersAsync()
    {
        if (_context.Users.Any(u => u.Role == UserRole.Teacher))
        {
            _logger.LogInformation("Teachers already exist, skipping seeding");
            return;
        }

        // Create the main teacher user (for easy login)
        var mainTeacher = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Teacher",
            LastName = "User",
            Email = "teacher@projectathena.com",
            UserName = "teacher@projectathena.com",
            EmailConfirmed = true,
            Role = UserRole.Teacher,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(mainTeacher, _config.DefaultPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(mainTeacher, "Teacher");
            _logger.LogInformation("Created main teacher: {Email}", mainTeacher.Email);
        }
        else
        {
            _logger.LogError("Failed to create main teacher {Email}: {Errors}", mainTeacher.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Create additional realistic teacher users using Faker
        var teacherFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName, "projectathena.com"))
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.EmailConfirmed, f => true)
            .RuleFor(u => u.Role, f => UserRole.Teacher)
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime)
            .RuleFor(u => u.IsActive, f => true);

        var additionalTeachers = teacherFaker.Generate(_config.TeacherCount - 1); // -1 because we already created the main teacher

        foreach (var teacher in additionalTeachers)
        {
            var createResult = await _userManager.CreateAsync(teacher, _config.DefaultPassword);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(teacher, "Teacher");
                _logger.LogInformation("Created teacher: {Email}", teacher.Email);
            }
            else
            {
                _logger.LogError("Failed to create teacher {Email}: {Errors}", teacher.Email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation("Completed seeding {Count} teacher users", additionalTeachers.Count + 1);
    }

    private async Task SeedStudentsAsync()
    {
        if (_context.Users.Any(u => u.Role == UserRole.Student))
        {
            _logger.LogInformation("Students already exist, skipping seeding");
            return;
        }

        // Create the main student user (for easy login)
        var mainStudent = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "Student",
            LastName = "User",
            Email = "student@projectathena.com",
            UserName = "student@projectathena.com",
            EmailConfirmed = true,
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(mainStudent, _config.DefaultPassword);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(mainStudent, "Student");
            _logger.LogInformation("Created main student: {Email}", mainStudent.Email);
        }
        else
        {
            _logger.LogError("Failed to create main student {Email}: {Errors}", mainStudent.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Create additional realistic student users using Faker
        var studentFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName, "projectathena.com"))
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.EmailConfirmed, f => true)
            .RuleFor(u => u.Role, f => UserRole.Student)
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime)
            .RuleFor(u => u.IsActive, f => true);

        var additionalStudents = studentFaker.Generate(_config.StudentCount - 1); // -1 because we already created the main student

        foreach (var student in additionalStudents)
        {
            var createResult = await _userManager.CreateAsync(student, _config.DefaultPassword);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(student, "Student");
                _logger.LogInformation("Created student: {Email}", student.Email);
            }
            else
            {
                _logger.LogError("Failed to create student {Email}: {Errors}", student.Email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        _logger.LogInformation("Completed seeding {Count} student users", additionalStudents.Count + 1);
    }
}