using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using ProjectAthena.Dtos.Auth;
using ProjectAthena.MinimalApi.ApiServices.Interfaces;
using System.Security.Claims;

namespace ProjectAthena.MinimalApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // Public endpoints
        authGroup.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("User login")
            .WithDescription("Authenticate user with email and password")
            .Produces<LoginResponseDto>()
            .ProducesValidationProblem()
            .Produces(401);

        authGroup.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("User registration")
            .WithDescription("Register a new user account")
            .Produces<UserDto>()
            .ProducesValidationProblem()
            .Produces(400);

        authGroup.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .WithDescription("Get a new access token using refresh token")
            .Produces<LoginResponseDto>()
            .Produces(401);

        // Protected endpoints
        var protectedAuthGroup = authGroup.RequireAuthorization();

        protectedAuthGroup.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("User logout")
            .WithDescription("Logout current user and revoke refresh token")
            .Produces(200)
            .Produces(401);

        protectedAuthGroup.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user")
            .WithDescription("Get current authenticated user information")
            .Produces<UserDto>()
            .Produces(401);

        protectedAuthGroup.MapPost("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Change password")
            .WithDescription("Change current user's password")
            .Produces<UserDto>()
            .ProducesValidationProblem()
            .Produces(401)
            .Produces(400);

        // Admin only endpoints
        var adminGroup = app.MapGroup("/api/admin/auth")
            .WithTags("Admin Authentication")
            .RequireAuthorization("Admin")
            .WithOpenApi();

        adminGroup.MapGet("/users", GetAllUsersAsync)
            .WithName("GetAllUsers")
            .WithSummary("Get all users")
            .WithDescription("Get list of all users (Admin only)")
            .Produces<List<UserDto>>()
            .Produces(401)
            .Produces(403);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequestDto request,
        IAuthService authService,
        IValidator<LoginRequestDto> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: 401,
                title: "Authentication failed",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: 500,
                title: "Login failed",
                detail: "An error occurred during login");
        }
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequestDto request,
        IAuthService authService,
        IValidator<RegisterRequestDto> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var user = await authService.RegisterAsync(request);
            return Results.Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                statusCode: 400,
                title: "Registration failed",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: 500,
                title: "Registration failed",
                detail: "An error occurred during registration");
        }
    }

    private static async Task<IResult> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        IAuthService authService)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Results.Problem(
                statusCode: 400,
                title: "Invalid request",
                detail: "Refresh token is required");
        }

        try
        {
            var response = await authService.RefreshTokenAsync(request);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: 401,
                title: "Token refresh failed",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: 500,
                title: "Token refresh failed",
                detail: "An error occurred during token refresh");
        }
    }

    private static async Task<IResult> LogoutAsync(
        ClaimsPrincipal user,
        IAuthService authService)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Problem(
                statusCode: 401,
                title: "Unauthorized",
                detail: "User ID not found in token");
        }

        try
        {
            await authService.LogoutAsync(userId);
            return Results.Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: 500,
                title: "Logout failed",
                detail: "An error occurred during logout");
        }
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ClaimsPrincipal user,
        IAuthService authService)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Problem(
                statusCode: 401,
                title: "Unauthorized",
                detail: "User ID not found in token");
        }

        try
        {
            var currentUser = await authService.GetCurrentUserAsync(userId);
            return Results.Ok(currentUser);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: 401,
                title: "Unauthorized",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: 500,
                title: "Failed to get user",
                detail: "An error occurred while retrieving user information");
        }
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequestDto request,
        ClaimsPrincipal user,
        IAuthService authService,
        IValidator<ChangePasswordRequestDto> validator)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Problem(
                statusCode: 401,
                title: "Unauthorized",
                detail: "User ID not found in token");
        }

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        try
        {
            var updatedUser = await authService.ChangePasswordAsync(userId, request);
            return Results.Ok(updatedUser);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                statusCode: 401,
                title: "Unauthorized",
                detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                statusCode: 400,
                title: "Password change failed",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                statusCode: 500,
                title: "Password change failed",
                detail: "An error occurred while changing password");
        }
    }

    private static async Task<IResult> GetAllUsersAsync(IAuthService authService)
    {
        // This would require a separate service method for admin operations
        // For now, return a placeholder
        return Results.Problem(
            statusCode: 501,
            title: "Not implemented",
            detail: "Admin user management not yet implemented");
    }
}