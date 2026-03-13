import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  ForecastResponse,
  LocationSearchResult,
  WeatherResponse
} from '../models/weather.model';

/**
 * Service for weather data retrieval from the backend API.
 * Provides methods to get current weather, forecasts, and city search results.
 */
@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private readonly baseUrl = `${environment.apiUrl}/weather`;

  constructor(private http: HttpClient) {}

  /**
   * Gets the current weather conditions for the specified city.
   * @param cityName The name of the city to retrieve weather for.
   * @returns Observable emitting the current weather response data.
   */
  getCurrentWeather(cityName: string): Observable<WeatherResponse> {
    return this.http
      .get<ApiResponse<WeatherResponse>>(`${this.baseUrl}/current/${encodeURIComponent(cityName)}`)
      .pipe(map(response => response.data));
  }

  /**
   * Gets the weather forecast (hourly and 5-day extended) for the specified city.
   * @param cityName The name of the city to retrieve forecast for.
   * @returns Observable emitting the forecast response data.
   */
  getForecast(cityName: string): Observable<ForecastResponse> {
    return this.http
      .get<ApiResponse<ForecastResponse>>(`${this.baseUrl}/forecast/${encodeURIComponent(cityName)}`)
      .pipe(map(response => response.data));
  }

  /**
   * Searches for cities matching the provided query string.
   * @param query The partial city name to search (minimum 2 characters).
   * @returns Observable emitting the list of matching city results.
   */
  searchCities(query: string): Observable<LocationSearchResult[]> {
    return this.http
      .get<ApiResponse<LocationSearchResult[]>>(`${this.baseUrl}/search/${encodeURIComponent(query)}`)
      .pipe(map(response => response.data));
  }
}
