using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ProjectAthena.Dtos.Enrollments;
using ProjectAthena.Dtos.Common;
using ProjectAthena.Data.Models;

namespace AspireJavaScript.MinimalApi.Endpoints;

public static class EnrollmentEndpoints
{
    public static void MapEnrollmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/enrollments")
            .WithTags("Enrollments")
            .WithOpenApi();

        group.MapGet("/", GetAllEnrollments)
            .WithName("GetAllEnrollments")
            .WithSummary("Get all enrollments with search and pagination")
            .Produces<PagedResult<EnrollmentDto>>(200)
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetEnrollmentById)
            .WithName("GetEnrollmentById")
            .WithSummary("Get enrollment by ID")
            .Produces<EnrollmentDto>(200)
            .Produces(404)
            .RequireAuthorization("Student");

        group.MapGet("/student/{studentId}", GetEnrollmentsByStudent)
            .WithName("GetEnrollmentsByStudent")
            .WithSummary("Get enrollments by student ID")
            .Produces<IEnumerable<EnrollmentDto>>(200)
            .RequireAuthorization("Student");

        group.MapGet("/course/{courseId:guid}", GetEnrollmentsByCourse)
            .WithName("GetEnrollmentsByCourse")
            .WithSummary("Get enrollments by course ID")
            .Produces<IEnumerable<EnrollmentDto>>(200)
            .RequireAuthorization("Teacher");

        group.MapPost("/", CreateEnrollment)
            .WithName("CreateEnrollment")
            .WithSummary("Create a new enrollment (assign student to course)")
            .Produces<EnrollmentDto>(201)
            .Produces(400)
            .RequireAuthorization("Admin");

        group.MapPatch("/{id:guid}/status", UpdateEnrollmentStatus)
            .WithName("UpdateEnrollmentStatus")
            .WithSummary("Update enrollment status and grade")
            .Produces(200)
            .Produces(404)
            .Produces(400)
            .RequireAuthorization("Teacher");

        group.MapDelete("/{id:guid}", DeleteEnrollment)
            .WithName("DeleteEnrollment")
            .WithSummary("Delete an enrollment")
            .Produces(204)
            .Produces(404)
            .RequireAuthorization("Admin");

        group.MapGet("/report/{courseId:guid}", GenerateEnrollmentReport)
            .WithName("GenerateEnrollmentReport")
            .WithSummary("Generate enrollment report for a course")
            .Produces<EnrollmentReportDto>(200)
            .Produces(404)
            .RequireAuthorization("Teacher");

        group.MapPost("/reports/advanced", GenerateAdvancedEnrollmentReport)
            .WithName("GenerateAdvancedEnrollmentReport")  
            .WithSummary("Generate advanced enrollment report with filters")
            .Produces<EnrollmentReportResponseDto>(200)
            .Produces(400)
            .RequireAuthorization("Teacher");

        group.MapGet("/check/{studentId}/{courseId:guid}", CheckEnrollmentStatus)
            .WithName("CheckEnrollmentStatus")
            .WithSummary("Check if student is enrolled in a course")
            .Produces<EnrollmentCheckDto>(200)
            .AllowAnonymous();
    }

    private static async Task<IResult> GetAllEnrollments(
        IEnrollmentService enrollmentService,
        string? search = null,
        int? status = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var enrollments = await enrollmentService.GetAllEnrollmentsAsync(search, status, page, pageSize);
            return Results.Ok(enrollments);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving enrollments",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetEnrollmentById(Guid id, IEnrollmentService enrollmentService)
    {
        try
        {
            var enrollment = await enrollmentService.GetEnrollmentByIdAsync(id);
            return enrollment != null ? Results.Ok(enrollment) : Results.NotFound($"Enrollment with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving enrollment",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetEnrollmentsByStudent(string studentId, IEnrollmentService enrollmentService)
    {
        try
        {
            var enrollments = await enrollmentService.GetEnrollmentsByStudentIdAsync(studentId);
            return Results.Ok(enrollments);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving student enrollments",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetEnrollmentsByCourse(Guid courseId, IEnrollmentService enrollmentService)
    {
        try
        {
            var enrollments = await enrollmentService.GetEnrollmentsByCourseIdAsync(courseId);
            return Results.Ok(enrollments);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving course enrollments",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> CreateEnrollment(CreateEnrollmentDto createEnrollmentDto, IEnrollmentService enrollmentService)
    {
        try
        {
            var enrollment = await enrollmentService.CreateEnrollmentAsync(createEnrollmentDto);
            return Results.Created($"/api/enrollments/{enrollment.Id}", enrollment);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error creating enrollment",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateEnrollmentStatus(
        Guid id, 
        UpdateEnrollmentStatusDto updateDto, 
        IEnrollmentService enrollmentService)
    {
        try
        {
            var updated = await enrollmentService.UpdateEnrollmentStatusAsync(id, updateDto.Status, updateDto.Grade);
            return updated ? Results.Ok() : Results.NotFound($"Enrollment with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error updating enrollment status",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteEnrollment(Guid id, IEnrollmentService enrollmentService)
    {
        try
        {
            var deleted = await enrollmentService.DeleteEnrollmentAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound($"Enrollment with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error deleting enrollment",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GenerateEnrollmentReport(Guid courseId, IEnrollmentService enrollmentService)
    {
        try
        {
            var report = await enrollmentService.GenerateEnrollmentReportAsync(courseId);
            return report != null ? Results.Ok(report) : Results.NotFound($"Course with ID {courseId} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error generating enrollment report",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GenerateAdvancedEnrollmentReport(EnrollmentReportRequestDto request, IEnrollmentService enrollmentService)
    {
        try
        {
            var report = await enrollmentService.GenerateAdvancedEnrollmentReportAsync(request);
            return Results.Ok(report);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error generating advanced enrollment report",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> CheckEnrollmentStatus(string studentId, Guid courseId, IEnrollmentService enrollmentService)
    {
        try
        {
            var isCurrentlyEnrolled = await enrollmentService.IsStudentEnrolledAsync(studentId, courseId);
            var hasEverEnrolled = await enrollmentService.HasStudentEverEnrolledAsync(studentId, courseId);
            
            var result = new EnrollmentCheckDto
            {
                StudentId = studentId,
                CourseId = courseId,
                IsCurrentlyEnrolled = isCurrentlyEnrolled,
                HasEverEnrolled = hasEverEnrolled
            };
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error checking enrollment status",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}

public record UpdateEnrollmentStatusDto(EnrollmentStatus Status, decimal? Grade);

public record EnrollmentCheckDto
{
    public string StudentId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public bool IsCurrentlyEnrolled { get; set; }
    public bool HasEverEnrolled { get; set; }
};