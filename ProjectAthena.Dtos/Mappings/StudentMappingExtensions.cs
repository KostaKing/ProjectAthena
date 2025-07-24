using ProjectAthena.Data.Models.Students;
using ProjectAthena.Dtos.Students;

namespace ProjectAthena.Dtos.Mappings;

public static class StudentMappingExtensions
{
    public static StudentDto ToDto(this Student student)
    {
        return new StudentDto
        {
            Id = student.Id,
            UserId = student.UserId,
            UserName = student.User?.UserName ?? string.Empty,
            Email = student.User?.Email ?? string.Empty,
            FirstName = student.User?.FirstName ?? string.Empty,
            LastName = student.User?.LastName ?? string.Empty,
            FullName = student.User?.FullName ?? string.Empty,
            StudentNumber = student.StudentNumber,
            DateOfBirth = student.DateOfBirth,
            Phone = student.Phone,
            Address = student.Address,
            EmergencyContact = student.EmergencyContact,
            EmergencyContactPhone = student.EmergencyContactPhone,
            EnrollmentDate = student.EnrollmentDate,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            IsActive = student.IsActive
        };
    }

    public static Student ToEntity(this CreateStudentDto dto)
    {
        var enrollmentDate = dto.EnrollmentDate ?? DateTime.UtcNow;
        return new Student
        {
            UserId = dto.UserId,
            StudentNumber = dto.StudentNumber,
            DateOfBirth = dto.DateOfBirth.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc) : dto.DateOfBirth.ToUniversalTime(),
            Phone = dto.Phone,
            Address = dto.Address,
            EmergencyContact = dto.EmergencyContact,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            EnrollmentDate = enrollmentDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(enrollmentDate, DateTimeKind.Utc) : enrollmentDate.ToUniversalTime()
        };
    }

    public static void UpdateEntity(this UpdateStudentDto dto, Student student)
    {
        student.Phone = dto.Phone;
        student.Address = dto.Address;
        student.EmergencyContact = dto.EmergencyContact;
        student.EmergencyContactPhone = dto.EmergencyContactPhone;
        student.UpdatedAt = DateTime.UtcNow;
    }
}