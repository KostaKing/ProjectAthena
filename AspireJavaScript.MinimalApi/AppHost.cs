using ProjectAthena.Data;
using ProjectAthena.Dtos;
using ProjectAthena.MinimalApi.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
