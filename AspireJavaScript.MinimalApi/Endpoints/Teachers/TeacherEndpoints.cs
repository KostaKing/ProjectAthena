using AspireJavaScript.MinimalApi.ApiServices.Interfaces.Teachers;
using Microsoft.AspNetCore.Authorization;
using ProjectAthena.Dtos.Teachers;

namespace AspireJavaScript.MinimalApi.Endpoints.Teachers;

public static class TeacherEndpoints
{
    public static void MapTeacherEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/teachers")
            .WithTags("Teachers")
            .WithOpenApi();

        group.MapGet("/", GetAllTeachers)
            .WithName("GetAllTeachers")
            .WithSummary("Get all teachers")
            .Produces<IEnumerable<TeacherDto>>(200)
            .RequireAuthorization("Teacher");

        group.MapGet("/{id:guid}", GetTeacherById)
            .WithName("GetTeacherById")
            .WithSummary("Get teacher by ID")
            .Produces<TeacherDto>(200)
            .Produces(404)
            .RequireAuthorization("Student");

        group.MapGet("/user/{userId}", GetTeacherByUserId)
            .WithName("GetTeacherByUserId")
            .WithSummary("Get teacher by user ID")
            .Produces<TeacherDto>(200)
            .Produces(404)
            .RequireAuthorization("Teacher");

        group.MapGet("/employee/{employeeNumber}", GetTeacherByEmployeeNumber)
            .WithName("GetTeacherByEmployeeNumber")
            .WithSummary("Get teacher by employee number")
            .Produces<TeacherDto>(200)
            .Produces(404)
            .RequireAuthorization("Teacher");

        group.MapPost("/", CreateTeacher)
            .WithName("CreateTeacher")
            .WithSummary("Create a new teacher")
            .Produces<TeacherDto>(201)
            .Produces(400)
            .RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", UpdateTeacher)
            .WithName("UpdateTeacher")
            .WithSummary("Update an existing teacher")
            .Produces<TeacherDto>(200)
            .Produces(404)
            .Produces(400)
            .RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", DeleteTeacher)
            .WithName("DeleteTeacher")
            .WithSummary("Delete a teacher")
            .Produces(204)
            .Produces(404)
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> GetAllTeachers(ITeacherService teacherService)
    {
        try
        {
            var teachers = await teacherService.GetAllTeachersAsync();
            return Results.Ok(teachers);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving teachers",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetTeacherById(Guid id, ITeacherService teacherService)
    {
        try
        {
            var teacher = await teacherService.GetTeacherByIdAsync(id);
            return teacher != null ? Results.Ok(teacher) : Results.NotFound($"Teacher with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving teacher",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetTeacherByUserId(string userId, ITeacherService teacherService)
    {
        try
        {
            var teacher = await teacherService.GetTeacherByUserIdAsync(userId);
            return teacher != null ? Results.Ok(teacher) : Results.NotFound($"Teacher with user ID {userId} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving teacher",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetTeacherByEmployeeNumber(string employeeNumber, ITeacherService teacherService)
    {
        try
        {
            var teacher = await teacherService.GetTeacherByEmployeeNumberAsync(employeeNumber);
            return teacher != null ? Results.Ok(teacher) : Results.NotFound($"Teacher with employee number {employeeNumber} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving teacher",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> CreateTeacher(CreateTeacherDto createTeacherDto, ITeacherService teacherService)
    {
        try
        {
            var teacher = await teacherService.CreateTeacherAsync(createTeacherDto);
            return Results.Created($"/api/teachers/{teacher.Id}", teacher);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error creating teacher",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateTeacher(Guid id, UpdateTeacherDto updateTeacherDto, ITeacherService teacherService)
    {
        try
        {
            var teacher = await teacherService.UpdateTeacherAsync(id, updateTeacherDto);
            return teacher != null ? Results.Ok(teacher) : Results.NotFound($"Teacher with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error updating teacher",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteTeacher(Guid id, ITeacherService teacherService)
    {
        try
        {
            var deleted = await teacherService.DeleteTeacherAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound($"Teacher with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error deleting teacher",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}