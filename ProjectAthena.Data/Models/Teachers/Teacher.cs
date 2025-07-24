namespace ProjectAthena.Data.Models.Teachers;

public class Teacher : BaseEntity
{
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public required string EmployeeNumber { get; set; }
    public string? Department { get; set; }
    public string? Title { get; set; }
    public string? Qualifications { get; set; }
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public string? OfficeLocation { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}