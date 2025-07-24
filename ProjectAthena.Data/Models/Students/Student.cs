namespace ProjectAthena.Data.Models.Students;

public class Student : BaseEntity
{
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public required string StudentNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}