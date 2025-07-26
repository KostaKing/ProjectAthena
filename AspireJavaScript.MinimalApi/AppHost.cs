using AspireJavaScript.MinimalApi.ApiServices.Interfaces;
using AspireJavaScript.MinimalApi.ApiServices.Services;
using AspireJavaScript.MinimalApi.Endpoints;
using ProjectAthena.MinimalApi.ApiServices.Interfaces;
using ProjectAthena.MinimalApi.ApiServices.Services;
using ProjectAthena.MinimalApi.Endpoints;
using AspireJavaScript.MinimalApi.ApiServices.Interfaces.Students;
using AspireJavaScript.MinimalApi.ApiServices.Services.Students;
using AspireJavaScript.MinimalApi.ApiServices.Interfaces.Teachers;
using AspireJavaScript.MinimalApi.ApiServices.Services.Teachers;
using AspireJavaScript.MinimalApi.Endpoints.Students;
using AspireJavaScript.MinimalApi.Endpoints.Teachers;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using System.Text;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add PostgreSQL DbContext
builder.AddNpgsqlDbContext<ApplicationDbContext>("ProjectAthenaDB");

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "ProjectAthena-SuperSecretKey-ForDevelopment-MinimumLength32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProjectAthena.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ProjectAthena.Client";

// Log JWT configuration for debugging
var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("JwtConfig");
logger.LogInformation("JWT Configuration - Issuer: {Issuer}, Audience: {Audience}, SecretKey Length: {KeyLength}", 
    jwtIssuer, jwtAudience, jwtSecretKey.Length);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RequireSignedTokens = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT Authentication failed: {Exception}", context.Exception?.Message);
            logger.LogError("JWT Token: {Token}", context.Request.Headers.Authorization.FirstOrDefault());
            logger.LogError("JWT Exception Details: {ExceptionType} - {InnerException}", 
                context.Exception?.GetType().Name, context.Exception?.InnerException?.Message);
            logger.LogError("JWT Full Exception: {FullException}", context.Exception?.ToString());
            
            // Log validation parameters for debugging
            var validationParams = context.Options.TokenValidationParameters;
            logger.LogError("JWT Validation Config - ValidateIssuer: {ValidateIssuer}, ValidIssuer: {ValidIssuer}", 
                validationParams.ValidateIssuer, validationParams.ValidIssuer);
            logger.LogError("JWT Validation Config - ValidateAudience: {ValidateAudience}, ValidAudience: {ValidAudience}", 
                validationParams.ValidateAudience, validationParams.ValidAudience);
            logger.LogError("JWT Validation Config - ValidateLifetime: {ValidateLifetime}, ValidateIssuerSigningKey: {ValidateIssuerSigningKey}", 
                validationParams.ValidateLifetime, validationParams.ValidateIssuerSigningKey);
            
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT Token validated successfully for user: {User}", 
                context.Principal?.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            
            // Check for test token header first (for Aspire testing)
            if (context.Request.Headers.TryGetValue("X-Test-Authorization", out var testToken))
            {
                context.Token = testToken.ToString().Replace("Bearer ", "");
                logger.LogInformation("Using test authorization header");
            }
            
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            logger.LogInformation("Authorization Header: {AuthHeader}", authHeader ?? "NULL");
            logger.LogInformation("JWT Token received: {Token}", context.Token?.Substring(0, Math.Min(50, context.Token?.Length ?? 0)) + "...");
            
            // Log if token is null or empty
            if (string.IsNullOrEmpty(context.Token))
            {
                logger.LogWarning("JWT Token is null or empty!");
            }
            
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("JWT Challenge triggered: {Error}, {ErrorDescription}", context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher", "Admin"));
    options.AddPolicy("Student", policy => policy.RequireRole("Student", "Teacher", "Admin"));
});

// Add application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddMemoryCache();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddCors();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(static builder => 
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Map authentication endpoints
app.MapAuthEndpoints();

// Map course and enrollment endpoints
app.MapCourseEndpoints();
app.MapEnrollmentEndpoints();

// Map student and teacher endpoints
app.MapStudentEndpoints();
app.MapTeacherEndpoints();

// Map dashboard endpoints
app.MapDashboardEndpoints();


app.Run();
