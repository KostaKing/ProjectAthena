using ProjectAthena.Data;
using ProjectAthena.Dtos;

namespace ProjectAthena.MinimalApi.Mappings;

public static class WeatherForecastMappings
{
    public static WeatherForecastDto ToDto(this WeatherForecast entity)
    {
        return new WeatherForecastDto(
            entity.Date,
            entity.TemperatureC,
            entity.TemperatureF,
            entity.Summary,
            entity.Location,
            entity.Humidity,
            entity.WindSpeed
        );
    }

    public static WeatherForecast ToEntity(this CreateWeatherForecastDto dto)
    {
        return new WeatherForecast
        {
            Date = dto.Date,
            TemperatureC = dto.TemperatureC,
            Summary = dto.Summary,
            Location = dto.Location,
            Humidity = dto.Humidity,
            WindSpeed = dto.WindSpeed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(this UpdateWeatherForecastDto dto, WeatherForecast entity)
    {
        entity.TemperatureC = dto.TemperatureC;
        entity.Summary = dto.Summary;
        entity.Location = dto.Location;
        entity.Humidity = dto.Humidity;
        entity.WindSpeed = dto.WindSpeed;
        entity.UpdatedAt = DateTime.UtcNow;
    }
}