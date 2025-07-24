using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ProjectAthena.Dtos.Enrollments;
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
            .WithSummary("Get all enrollments")
            .Produces<IEnumerable<EnrollmentDto>>(200)
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetEnrollmentById)
            .WithName("GetEnrollmentById")
            .WithSummary("Get enrollment by ID")
            .Produces<EnrollmentDto>(200)
            .Produces(404)
            .RequireAuthorization();

        group.MapGet("/student/{studentId}", GetEnrollmentsByStudent)
            .WithName("GetEnrollmentsByStudent")
            .WithSummary("Get enrollments by student ID")
            .Produces<IEnumerable<EnrollmentDto>>(200)
            .RequireAuthorization();

        group.MapGet("/course/{courseId:guid}", GetEnrollmentsByCourse)
            .WithName("GetEnrollmentsByCourse")
            .WithSummary("Get enrollments by course ID")
            .Produces<IEnumerable<EnrollmentDto>>(200)
            .RequireAuthorization();

        group.MapPost("/", CreateEnrollment)
            .WithName("CreateEnrollment")
            .WithSummary("Create a new enrollment (assign student to course)")
            .Produces<EnrollmentDto>(201)
            .Produces(400)
            .RequireAuthorization();

        group.MapPatch("/{id:guid}/status", UpdateEnrollmentStatus)
            .WithName("UpdateEnrollmentStatus")
            .WithSummary("Update enrollment status and grade")
            .Produces(200)
            .Produces(404)
            .Produces(400)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteEnrollment)
            .WithName("DeleteEnrollment")
            .WithSummary("Delete an enrollment")
            .Produces(204)
            .Produces(404)
            .RequireAuthorization();

        group.MapGet("/report/{courseId:guid}", GenerateEnrollmentReport)
            .WithName("GenerateEnrollmentReport")
            .WithSummary("Generate enrollment report for a course")
            .Produces<EnrollmentReportDto>(200)
            .Produces(404)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAllEnrollments(IEnrollmentService enrollmentService)
    {
        try
        {
            var enrollments = await enrollmentService.GetAllEnrollmentsAsync();
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
}

public record UpdateEnrollmentStatusDto(EnrollmentStatus Status, decimal? Grade);