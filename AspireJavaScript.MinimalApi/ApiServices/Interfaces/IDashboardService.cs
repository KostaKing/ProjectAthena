using ProjectAthena.Dtos.Dashboard;

namespace AspireJavaScript.MinimalApi.ApiServices.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}