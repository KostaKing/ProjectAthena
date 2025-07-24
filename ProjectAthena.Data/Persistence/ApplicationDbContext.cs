using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectAthena.Data.Models;

namespace ProjectAthena.Data.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        // Configure BaseEntity properties for all entities
        builder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.CourseCode)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(e => e.Credits)
                .IsRequired();
            
            entity.Property(e => e.MaxEnrollments)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.HasIndex(e => e.CourseCode)
                .IsUnique();
            
            entity.HasOne(e => e.Instructor)
                .WithMany()
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
            
            entity.Property(e => e.EnrollmentDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.Grade)
                .HasPrecision(5, 2);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();
        });

        // Configure Identity tables
        builder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
        });

        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("AspNetUserClaims");
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("AspNetUserLogins");
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("AspNetUserTokens");
        });

        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("AspNetRoleClaims");
        });

        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("AspNetUserRoles");
        });

        // Seed default roles
        SeedRoles(builder);
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
            },
            new IdentityRole
            {
                Id = "2",
                Name = "Teacher",
                NormalizedName = "TEACHER",
                ConcurrencyStamp = "b2c3d4e5-f6g7-8901-bcde-f23456789012"
            },
            new IdentityRole
            {
                Id = "3",
                Name = "Student",
                NormalizedName = "STUDENT",
                ConcurrencyStamp = "c3d4e5f6-g7h8-9012-cdef-345678901234"
            }
        );
    }
}