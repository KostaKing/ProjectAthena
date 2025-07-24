using ProjectAthena.Data.Models.Teachers;
using ProjectAthena.Dtos.Teachers;

namespace ProjectAthena.Dtos.Mappings;

public static class TeacherMappingExtensions
{
    public static TeacherDto ToDto(this Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id,
            UserId = teacher.UserId,
            UserName = teacher.User?.UserName ?? string.Empty,
            Email = teacher.User?.Email ?? string.Empty,
            FirstName = teacher.User?.FirstName ?? string.Empty,
            LastName = teacher.User?.LastName ?? string.Empty,
            FullName = teacher.User?.FullName ?? string.Empty,
            EmployeeNumber = teacher.EmployeeNumber,
            Department = teacher.Department,
            Title = teacher.Title,
            Qualifications = teacher.Qualifications,
            Specialization = teacher.Specialization,
            Phone = teacher.Phone,
            OfficeLocation = teacher.OfficeLocation,
            HireDate = teacher.HireDate,
            CreatedAt = teacher.CreatedAt,
            UpdatedAt = teacher.UpdatedAt,
            IsActive = teacher.IsActive
        };
    }

    public static Teacher ToEntity(this CreateTeacherDto dto)
    {
        var hireDate = dto.HireDate ?? DateTime.UtcNow;
        return new Teacher
        {
            UserId = dto.UserId,
            EmployeeNumber = dto.EmployeeNumber,
            Department = dto.Department,
            Title = dto.Title,
            Qualifications = dto.Qualifications,
            Specialization = dto.Specialization,
            Phone = dto.Phone,
            OfficeLocation = dto.OfficeLocation,
            HireDate = hireDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(hireDate, DateTimeKind.Utc) : hireDate.ToUniversalTime()
        };
    }

    public static void UpdateEntity(this UpdateTeacherDto dto, Teacher teacher)
    {
        teacher.Department = dto.Department;
        teacher.Title = dto.Title;
        teacher.Qualifications = dto.Qualifications;
        teacher.Specialization = dto.Specialization;
        teacher.Phone = dto.Phone;
        teacher.OfficeLocation = dto.OfficeLocation;
        teacher.UpdatedAt = DateTime.UtcNow;
    }
}