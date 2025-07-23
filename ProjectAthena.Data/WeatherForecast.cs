namespace ProjectAthena.Data;

public class WeatherForecast
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    public string? Location { get; set; }
    public decimal Humidity { get; set; }
    public int WindSpeed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}