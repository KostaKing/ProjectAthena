using ProjectAthena.Data.Models.Students;

namespace ProjectAthena.Data.Models;

public class Enrollment : BaseEntity
{
    public required Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public decimal? Grade { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public enum EnrollmentStatus
{
    Active = 1,
    Completed = 2,
    Dropped = 3,
    Suspended = 4
}