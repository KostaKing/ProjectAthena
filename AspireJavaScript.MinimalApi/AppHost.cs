using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProjectAthena.Data;
using ProjectAthena.Data.Models;
using ProjectAthena.Data.Persistence;
using ProjectAthena.Dtos;
using ProjectAthena.MinimalApi.ApiServices.Interfaces;
using ProjectAthena.MinimalApi.ApiServices.Services;
using ProjectAthena.MinimalApi.Endpoints;
using ProjectAthena.MinimalApi.Mappings;
using System.Text;

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"))),
        ClockSkew = TimeSpan.Zero
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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var locations = new[]
{
    "New York", "London", "Tokyo", "Sydney", "Paris", "Berlin", "Moscow", "Cairo", "Mumbai", "Beijing"
};

// In-memory storage for demo purposes
var forecasts = new List<WeatherForecast>();
var nextId = 1;

app.MapGet("/weatherforecast", () =>
{
    if (!forecasts.Any())
    {
        // Generate initial data
        for (int i = 1; i <= 5; i++)
        {
            forecasts.Add(new WeatherForecast
            {
                Id = nextId++,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)],
                Location = locations[Random.Shared.Next(locations.Length)],
                Humidity = Random.Shared.Next(30, 100),
                WindSpeed = Random.Shared.Next(0, 50),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
    
    return forecasts.Select(f => f.ToDto()).ToArray();
})
.WithName("GetWeatherForecasts")
.WithOpenApi()
.WithSummary("Get all weather forecasts")
.WithDescription("Returns a list of weather forecasts");

app.MapGet("/weatherforecast/{id}", (int id) =>
{
    var forecast = forecasts.FirstOrDefault(f => f.Id == id);
    return forecast is not null ? Results.Ok(forecast.ToDto()) : Results.NotFound();
})
.WithName("GetWeatherForecastById")
.WithOpenApi()
.WithSummary("Get weather forecast by ID")
.WithDescription("Returns a specific weather forecast by its ID");

app.MapPost("/weatherforecast", (CreateWeatherForecastDto dto) =>
{
    var forecast = dto.ToEntity();
    forecast.Id = nextId++;
    forecasts.Add(forecast);
    
    return Results.Created($"/weatherforecast/{forecast.Id}", forecast.ToDto());
})
.WithName("CreateWeatherForecast")
.WithOpenApi()
.WithSummary("Create a new weather forecast")
.WithDescription("Creates a new weather forecast with the provided data");

app.MapPut("/weatherforecast/{id}", (int id, UpdateWeatherForecastDto dto) =>
{
    var forecast = forecasts.FirstOrDefault(f => f.Id == id);
    if (forecast is null)
        return Results.NotFound();
    
    dto.UpdateEntity(forecast);
    return Results.Ok(forecast.ToDto());
})
.WithName("UpdateWeatherForecast")
.WithOpenApi()
.WithSummary("Update weather forecast")
.WithDescription("Updates an existing weather forecast");

app.MapDelete("/weatherforecast/{id}", (int id) =>
{
    var forecast = forecasts.FirstOrDefault(f => f.Id == id);
    if (forecast is null)
        return Results.NotFound();
    
    forecasts.Remove(forecast);
    return Results.NoContent();
})
.WithName("DeleteWeatherForecast")
.WithOpenApi()
.WithSummary("Delete weather forecast")
.WithDescription("Deletes a weather forecast by ID");

app.Run();
