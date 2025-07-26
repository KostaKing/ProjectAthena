using Microsoft.IdentityModel.Tokens;
using ProjectAthena.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Http.Json;

namespace ProjectAthena.Tests;

public static class TestAuthenticationHelper
{
    private const string TestSecretKey = "ProjectAthena-SuperSecretKey-ForDevelopment-MinimumLength32Characters!";
    private const string TestIssuer = "ProjectAthena.Api";
    private const string TestAudience = "ProjectAthena.Client";

    public static string GenerateTestJwtToken(string userId = "test-user-id", string email = "test@example.com", UserRole role = UserRole.Admin, string firstName = "Test", string lastName = "User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.GivenName, firstName),
            new(ClaimTypes.Surname, lastName),
            new(ClaimTypes.Role, role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateAdminToken() => GenerateTestJwtToken(role: UserRole.Admin);
    public static string GenerateTeacherToken() => GenerateTestJwtToken(role: UserRole.Teacher);
    public static string GenerateStudentToken() => GenerateTestJwtToken(role: UserRole.Student);

    public static void AddAuthenticationHeader(this HttpClient httpClient, string token)
    {
        httpClient.DefaultRequestHeaders.Remove("Authorization");
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<HttpResponseMessage> PostAsJsonWithAuthAsync<T>(this HttpClient httpClient, string requestUri, T value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add("X-Test-Authorization", $"Bearer {token}");
        return await httpClient.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> GetWithAuthAsync(this HttpClient httpClient, string requestUri, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("X-Test-Authorization", $"Bearer {token}");
        return await httpClient.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> PatchAsJsonWithAuthAsync<T>(this HttpClient httpClient, string requestUri, T value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        request.Headers.Add("X-Test-Authorization", $"Bearer {token}");
        return await httpClient.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> DeleteWithAuthAsync(this HttpClient httpClient, string requestUri, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Add("X-Test-Authorization", $"Bearer {token}");
        return await httpClient.SendAsync(request);
    }

    public static void AddAdminAuth(this HttpClient httpClient) => httpClient.AddAuthenticationHeader(GenerateAdminToken());
    public static void AddTeacherAuth(this HttpClient httpClient) => httpClient.AddAuthenticationHeader(GenerateTeacherToken());
    public static void AddStudentAuth(this HttpClient httpClient) => httpClient.AddAuthenticationHeader(GenerateStudentToken());
}