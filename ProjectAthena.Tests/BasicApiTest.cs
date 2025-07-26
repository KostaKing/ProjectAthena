using System.Net;

namespace ProjectAthena.Tests;

[Collection("AspireApp")]
public class BasicApiTest
{
    private readonly AspireAppFixture _fixture;
    private readonly HttpClient _httpClient;

    public BasicApiTest(AspireAppFixture fixture)
    {
        _fixture = fixture;
        _httpClient = _fixture.App.CreateHttpClient("ProjectAthenaApi");
    }

    [Fact]
    public async Task Api_ShouldBeReachable()
    {
        // This should work without authentication
        var response = await _httpClient.GetAsync("/swagger/v1/swagger.json");
        
        // Should get 200 for swagger endpoint
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Api_ShouldReturn200_WhenNoAuthRequired()
    {
        // This should return 200 since we made it AllowAnonymous
        var response = await _httpClient.GetAsync("/api/enrollments");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Api_ShouldAcceptValidJwtToken()
    {
        // Generate and inspect the token
        var token = TestAuthenticationHelper.GenerateTeacherToken();
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        // Log token details for debugging
        Console.WriteLine($"Token Issuer: {jsonToken.Issuer}");
        Console.WriteLine($"Token Audience: {string.Join(", ", jsonToken.Audiences)}");
        Console.WriteLine($"Token Expires: {jsonToken.ValidTo}");
        Console.WriteLine($"Token Claims: {string.Join(", ", jsonToken.Claims.Select(c => $"{c.Type}={c.Value}"))}");
        
        // Add valid JWT token
        _httpClient.AddTeacherAuth();
        
        var response = await _httpClient.GetAsync("/api/enrollments");
        
        // Should not be 401 anymore
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}