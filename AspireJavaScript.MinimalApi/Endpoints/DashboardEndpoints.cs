using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using ProjectAthena.Dtos.Dashboard;

namespace AspireJavaScript.MinimalApi.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/dashboard")
            .WithTags("Dashboard")
            .WithOpenApi();

        group.MapGet("/stats", GetDashboardStats)
            .WithName("GetDashboardStats")
            .WithSummary("Get dashboard statistics")
            .Produces<DashboardStatsDto>(200)
            .RequireAuthorization("Admin");
    }

    private static async Task<IResult> GetDashboardStats(IDashboardService dashboardService)
    {
        try
        {
            var stats = await dashboardService.GetDashboardStatsAsync();
            return Results.Ok(stats);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving dashboard statistics",
                detail: ex.Message,
                statusCode: 500);
        }
    }
}