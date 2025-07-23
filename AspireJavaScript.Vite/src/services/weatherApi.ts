import { components } from '../types/api';

type WeatherForecastDto = components['schemas']['WeatherForecastDto'];
type CreateWeatherForecastDto = components['schemas']['CreateWeatherForecastDto'];
type UpdateWeatherForecastDto = components['schemas']['UpdateWeatherForecastDto'];

const API_BASE = '/api';

class WeatherApiClient {
  async getForecasts(): Promise<WeatherForecastDto[]> {
    const response = await fetch(`${API_BASE}/weatherforecast`);
    if (!response.ok) {
      throw new Error(`Failed to fetch forecasts: ${response.statusText}`);
    }
    return response.json();
  }

  async getForecastById(id: number): Promise<WeatherForecastDto> {
    const response = await fetch(`${API_BASE}/weatherforecast/${id}`);
    if (!response.ok) {
      throw new Error(`Failed to fetch forecast ${id}: ${response.statusText}`);
    }
    return response.json();
  }

  async createForecast(forecast: CreateWeatherForecastDto): Promise<void> {
    const response = await fetch(`${API_BASE}/weatherforecast`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(forecast),
    });
    if (!response.ok) {
      throw new Error(`Failed to create forecast: ${response.statusText}`);
    }
  }

  async updateForecast(id: number, forecast: UpdateWeatherForecastDto): Promise<void> {
    const response = await fetch(`${API_BASE}/weatherforecast/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(forecast),
    });
    if (!response.ok) {
      throw new Error(`Failed to update forecast ${id}: ${response.statusText}`);
    }
  }

  async deleteForecast(id: number): Promise<void> {
    const response = await fetch(`${API_BASE}/weatherforecast/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error(`Failed to delete forecast ${id}: ${response.statusText}`);
    }
  }
}

export const weatherApi = new WeatherApiClient();
export type { WeatherForecastDto, CreateWeatherForecastDto, UpdateWeatherForecastDto };