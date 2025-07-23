namespace ProjectAthena.Dtos;

public record WeatherForecastDto(
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string? Summary,
    string? Location,
    decimal Humidity,
    int WindSpeed
);

public record CreateWeatherForecastDto(
    DateOnly Date,
    int TemperatureC,
    string? Summary,
    string? Location,
    decimal Humidity,
    int WindSpeed
);

public record UpdateWeatherForecastDto(
    int TemperatureC,
    string? Summary,
    string? Location,
    decimal Humidity,
    int WindSpeed
);