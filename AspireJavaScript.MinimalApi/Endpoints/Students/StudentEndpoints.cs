using AspireJavaScript.MinimalApi.ApiServices.Interfaces.Students;
using Microsoft.AspNetCore.Authorization;
using ProjectAthena.Dtos.Students;

namespace AspireJavaScript.MinimalApi.Endpoints.Students;

public static class StudentEndpoints
{
    public static void MapStudentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/students")
            .WithTags("Students")
            .WithOpenApi();

        group.MapGet("/", GetAllStudents)
            .WithName("GetAllStudents")
            .WithSummary("Get all students")
            .Produces<IEnumerable<StudentDto>>(200)
            .RequireAuthorization("Teacher");

        group.MapGet("/{id:guid}", GetStudentById)
            .WithName("GetStudentById")
            .WithSummary("Get student by ID")
            .Produces<StudentDto>(200)
            .Produces(404)
            .RequireAuthorization("Student");

        group.MapGet("/user/{userId}", GetStudentByUserId)
            .WithName("GetStudentByUserId")
            .WithSummary("Get student by user ID")
            .Produces<StudentDto>(200)
            .Produces(404)
            .RequireAuthorization("Student");

        group.MapGet("/number/{studentNumber}", GetStudentByStudentNumber)
            .WithName("GetStudentByStudentNumber")
            .WithSummary("Get student by student number")
            .Produces<StudentDto>(200)
            .Produces(404)
            .RequireAuthorization("Teacher");

        group.MapPost("/", CreateStudent)
            .WithName("CreateStudent")
            .WithSummary("Create a new student")
            .Produces<StudentDto>(201)
            .Produces(400)
            .RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", UpdateStudent)
            .WithName("UpdateStudent")
            .WithSummary("Update an existing student")
            .Produces<StudentDto>(200)
            .Produces(404)
            .Produces(400)
            .RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", DeleteStudent)
            .WithName("DeleteStudent")
            .WithSummary("Delete a student")
            .Produces(204)
            .Produces(404)
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> GetAllStudents(IStudentService studentService)
    {
        try
        {
            var students = await studentService.GetAllStudentsAsync();
            return Results.Ok(students);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving students",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentById(Guid id, IStudentService studentService)
    {
        try
        {
            var student = await studentService.GetStudentByIdAsync(id);
            return student != null ? Results.Ok(student) : Results.NotFound($"Student with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving student",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentByUserId(string userId, IStudentService studentService)
    {
        try
        {
            var student = await studentService.GetStudentByUserIdAsync(userId);
            return student != null ? Results.Ok(student) : Results.NotFound($"Student with user ID {userId} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving student",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetStudentByStudentNumber(string studentNumber, IStudentService studentService)
    {
        try
        {
            var student = await studentService.GetStudentByStudentNumberAsync(studentNumber);
            return student != null ? Results.Ok(student) : Results.NotFound($"Student with number {studentNumber} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving student",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> CreateStudent(CreateStudentDto createStudentDto, IStudentService studentService)
    {
        try
        {
            var student = await studentService.CreateStudentAsync(createStudentDto);
            return Results.Created($"/api/students/{student.Id}", student);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error creating student",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateStudent(Guid id, UpdateStudentDto updateStudentDto, IStudentService studentService)
    {
        try
        {
            var student = await studentService.UpdateStudentAsync(id, updateStudentDto);
            return student != null ? Results.Ok(student) : Results.NotFound($"Student with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error updating student",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteStudent(Guid id, IStudentService studentService)
    {
        try
        {
            var deleted = await studentService.DeleteStudentAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound($"Student with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error deleting student",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}