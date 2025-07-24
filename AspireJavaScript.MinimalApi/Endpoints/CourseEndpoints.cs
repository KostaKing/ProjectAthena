using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ProjectAthena.Dtos.Courses;

namespace AspireJavaScript.MinimalApi.Endpoints;

public static class CourseEndpoints
{
    public static void MapCourseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/courses")
            .WithTags("Courses")
            .WithOpenApi();

        group.MapGet("/", GetAllCourses)
            .WithName("GetAllCourses")
            .WithSummary("Get all courses")
            .Produces<IEnumerable<CourseDto>>(200);

        group.MapGet("/{id:guid}", GetCourseById)
            .WithName("GetCourseById")
            .WithSummary("Get course by ID")
            .Produces<CourseDto>(200)
            .Produces(404);

        group.MapGet("/code/{courseCode}", GetCourseByCode)
            .WithName("GetCourseByCode")
            .WithSummary("Get course by course code")
            .Produces<CourseDto>(200)
            .Produces(404);

        group.MapPost("/", CreateCourse)
            .WithName("CreateCourse")
            .WithSummary("Create a new course")
            .Produces<CourseDto>(201)
            .Produces(400)
            .RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", UpdateCourse)
            .WithName("UpdateCourse")
            .WithSummary("Update an existing course")
            .Produces<CourseDto>(200)
            .Produces(404)
            .Produces(400)
            .RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", DeleteCourse)
            .WithName("DeleteCourse")
            .WithSummary("Delete a course")
            .Produces(204)
            .Produces(404)
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> GetAllCourses(ICourseService courseService)
    {
        try
        {
            var courses = await courseService.GetAllCoursesAsync();
            return Results.Ok(courses);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving courses",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetCourseById(Guid id, ICourseService courseService)
    {
        try
        {
            var course = await courseService.GetCourseByIdAsync(id);
            return course != null ? Results.Ok(course) : Results.NotFound($"Course with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving course",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetCourseByCode(string courseCode, ICourseService courseService)
    {
        try
        {
            var course = await courseService.GetCourseByCodeAsync(courseCode);
            return course != null ? Results.Ok(course) : Results.NotFound($"Course with code {courseCode} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving course",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> CreateCourse(CreateCourseDto createCourseDto, ICourseService courseService)
    {
        try
        {
            var course = await courseService.CreateCourseAsync(createCourseDto);
            return Results.Created($"/api/courses/{course.Id}", course);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error creating course",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> UpdateCourse(Guid id, UpdateCourseDto updateCourseDto, ICourseService courseService)
    {
        try
        {
            var course = await courseService.UpdateCourseAsync(id, updateCourseDto);
            return course != null ? Results.Ok(course) : Results.NotFound($"Course with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error updating course",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> DeleteCourse(Guid id, ICourseService courseService)
    {
        try
        {
            var deleted = await courseService.DeleteCourseAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound($"Course with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error deleting course",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}