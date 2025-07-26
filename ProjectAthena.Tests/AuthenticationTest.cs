using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjectAthena.Tests;

public class AuthenticationTest
{
    [Fact]
    public void TestAuthenticationHelper_ShouldGenerateValidToken()
    {
        // Arrange & Act
        var token = TestAuthenticationHelper.GenerateAdminToken();
        
        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify token structure
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Equal("ProjectAthena.Api", jsonToken.Issuer);
        Assert.Equal("ProjectAthena.Client", jsonToken.Audiences.First());
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }
    
    [Fact]
    public void TestAuthenticationHelper_ShouldGenerateTeacherToken()
    {
        // Arrange & Act
        var token = TestAuthenticationHelper.GenerateTeacherToken();
        
        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Teacher");
    }
    
    [Fact]
    public void TestAuthenticationHelper_ShouldGenerateStudentToken()
    {
        // Arrange & Act
        var token = TestAuthenticationHelper.GenerateStudentToken();
        
        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        Assert.Contains(jsonToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Student");
    }
}