import React, { useEffect, useState } from 'react';
import { weatherApi, WeatherForecastDto } from '../../services/weatherApi';
import { useAuth } from '../../hooks/useAuth';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { RefreshCw, Thermometer, Droplets, Wind, MapPin } from 'lucide-react';
import { useToast } from '../ui/use-toast';

export function WeatherDashboard() {
  const { token } = useAuth();
  const { toast } = useToast();
  const [forecasts, setForecasts] = useState<WeatherForecastDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchForecasts = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await weatherApi.getForecasts();
      setForecasts(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch weather data';
      setError(errorMessage);
      toast({
        variant: "destructive",
        title: "Error loading weather data",
        description: errorMessage,
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchForecasts();
  }, []);

  const handleRefresh = () => {
    fetchForecasts();
  };

  if (loading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            Weather Forecasts
            <RefreshCw className="h-5 w-5 animate-spin" />
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600">Loading weather data...</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            Weather Forecasts
            <Button variant="outline" size="sm" onClick={handleRefresh}>
              <RefreshCw className="h-4 w-4 mr-2" />
              Retry
            </Button>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-center py-8">
            <p className="text-red-600 mb-4">Error: {error}</p>
            <Button onClick={handleRefresh} variant="outline">
              Try Again
            </Button>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          Weather Forecasts
          <Button variant="outline" size="sm" onClick={handleRefresh} disabled={loading}>
            <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
        </CardTitle>
      </CardHeader>
      <CardContent>
        {forecasts.length === 0 ? (
          <div className="text-center py-8">
            <p className="text-gray-600">No weather forecasts available</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {forecasts.map((forecast, index) => (
                <div
                  key={forecast.date || index}
                  className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-lg p-4 border border-blue-200"
                >
                  <div className="flex items-center justify-between mb-3">
                    <h3 className="font-semibold text-gray-900">
                      {forecast.date}
                    </h3>
                    <div className="flex items-center text-sm text-gray-600">
                      <MapPin className="h-3 w-3 mr-1" />
                      {forecast.location || 'Unknown'}
                    </div>
                  </div>
                  
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center">
                        <Thermometer className="h-4 w-4 text-red-500 mr-2" />
                        <span className="text-sm text-gray-600">Temperature</span>
                      </div>
                      <span className="font-semibold">
                        {forecast.temperatureC}°C / {forecast.temperatureF}°F
                      </span>
                    </div>
                    
                    {forecast.humidity !== undefined && (
                      <div className="flex items-center justify-between">
                        <div className="flex items-center">
                          <Droplets className="h-4 w-4 text-blue-500 mr-2" />
                          <span className="text-sm text-gray-600">Humidity</span>
                        </div>
                        <span className="font-semibold">{forecast.humidity}%</span>
                      </div>
                    )}
                    
                    {forecast.windSpeed !== undefined && (
                      <div className="flex items-center justify-between">
                        <div className="flex items-center">
                          <Wind className="h-4 w-4 text-gray-500 mr-2" />
                          <span className="text-sm text-gray-600">Wind</span>
                        </div>
                        <span className="font-semibold">{forecast.windSpeed} km/h</span>
                      </div>
                    )}
                    
                    <div className="pt-2 border-t border-blue-200">
                      <p className="text-center text-sm font-medium text-blue-800">
                        {forecast.summary}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}