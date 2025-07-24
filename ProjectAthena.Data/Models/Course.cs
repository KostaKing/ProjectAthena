namespace ProjectAthena.Data.Models;

public class Course : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string CourseCode { get; set; }
    public int Credits { get; set; }
    public string? InstructorId { get; set; }
    public ApplicationUser? Instructor { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxEnrollments { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}