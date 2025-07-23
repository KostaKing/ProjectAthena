import { useEffect, useState } from "react";
import "./App.css";
import { weatherApi, WeatherForecastDto } from "./services/weatherApi";

function App() {
  const [forecasts, setForecasts] = useState<WeatherForecastDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const requestWeather = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await weatherApi.getForecasts();
      setForecasts(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch weather data');
      console.error('Weather fetch error:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    requestWeather();
  }, []);

  if (loading) {
    return (
      <div className="App">
        <header className="App-header">
          <h1>React (Vite) Weather</h1>
          <p>Loading weather data...</p>
        </header>
      </div>
    );
  }

  if (error) {
    return (
      <div className="App">
        <header className="App-header">
          <h1>React (Vite) Weather</h1>
          <p style={{ color: 'red' }}>Error: {error}</p>
          <button onClick={requestWeather}>Retry</button>
        </header>
      </div>
    );
  }

  return (
    <div className="App">
      <header className="App-header">
        <h1>React (Vite) Weather</h1>
        <button onClick={requestWeather} style={{ marginBottom: '20px' }}>
          Refresh Weather
        </button>
        <table>
          <thead>
            <tr>
              <th>Date</th>
              <th>Temp. (C)</th>
              <th>Temp. (F)</th>
              <th>Summary</th>
              <th>Location</th>
              <th>Humidity</th>
              <th>Wind Speed</th>
            </tr>
          </thead>
          <tbody>
            {forecasts.length === 0 ? (
              <tr>
                <td colSpan={7}>No forecasts available</td>
              </tr>
            ) : (
              forecasts.map((forecast, index) => (
                <tr key={forecast.date || index}>
                  <td>{forecast.date}</td>
                  <td>{forecast.temperatureC}°</td>
                  <td>{forecast.temperatureF}°</td>
                  <td>{forecast.summary}</td>
                  <td>{forecast.location || 'N/A'}</td>
                  <td>{forecast.humidity ? `${forecast.humidity}%` : 'N/A'}</td>
                  <td>{forecast.windSpeed ? `${forecast.windSpeed} km/h` : 'N/A'}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </header>
    </div>
  );
}

export default App;
