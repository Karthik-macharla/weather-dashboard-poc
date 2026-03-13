import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { WeatherService } from './weather.service';
import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  ForecastResponse,
  LocationSearchResult,
  WeatherResponse
} from '../models/weather.model';

describe('WeatherService', () => {
  let service: WeatherService;
  let httpMock: HttpTestingController;

  const mockWeatherResponse: WeatherResponse = {
    location: {
      city: 'Toronto',
      state: 'Ontario',
      country: 'Canada',
      coordinates: { lat: 43.70, lon: -79.42 }
    },
    current: {
      temperature: { celsius: 22, fahrenheit: 72 },
      feelsLike: { celsius: 25, fahrenheit: 77 },
      humidity: 65,
      windSpeed: 12,
      pressure: 1013,
      description: 'Partly Cloudy',
      icon: 'partly-cloudy',
      timestamp: '2026-03-13T14:30:00Z'
    }
  };

  const mockForecastResponse: ForecastResponse = {
    location: mockWeatherResponse.location,
    todayHourly: [
      {
        time: '2026-03-13T15:00:00Z',
        temperature: { celsius: 23, fahrenheit: 73 },
        precipitationChance: 10,
        windSpeed: 15,
        description: 'Sunny'
      }
    ],
    extendedDaily: [
      {
        date: '2026-03-14',
        high: { celsius: 26, fahrenheit: 79 },
        low: { celsius: 18, fahrenheit: 64 },
        precipitationChance: 20,
        description: 'Mostly Sunny',
        icon: 'mostly-sunny'
      }
    ]
  };

  const mockSearchResults: LocationSearchResult[] = [
    {
      displayName: 'Toronto, Ontario, Canada',
      cityName: 'Toronto',
      state: 'Ontario',
      country: 'Canada',
      coordinates: { lat: 43.70, lon: -79.42 }
    }
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WeatherService]
    });
    service = TestBed.inject(WeatherService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCurrentWeather', () => {
    it('should call the correct URL and return weather data', () => {
      const apiResponse: ApiResponse<WeatherResponse> = {
        success: true,
        message: 'Weather data retrieved successfully',
        data: mockWeatherResponse
      };

      service.getCurrentWeather('Toronto').subscribe(data => {
        expect(data).toEqual(mockWeatherResponse);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/current/Toronto`);
      expect(req.request.method).toBe('GET');
      req.flush(apiResponse);
    });

    it('should URL-encode city names with special characters', () => {
      const apiResponse: ApiResponse<WeatherResponse> = {
        success: true,
        message: 'OK',
        data: mockWeatherResponse
      };

      service.getCurrentWeather('New York').subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/current/New%20York`);
      expect(req.request.method).toBe('GET');
      req.flush(apiResponse);
    });

    it('should propagate HTTP errors', () => {
      let errorReceived = false;

      service.getCurrentWeather('Unknown').subscribe({
        error: () => { errorReceived = true; }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/current/Unknown`);
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });

      expect(errorReceived).toBeTrue();
    });
  });

  describe('getForecast', () => {
    it('should call the correct URL and return forecast data', () => {
      const apiResponse: ApiResponse<ForecastResponse> = {
        success: true,
        message: 'Forecast data retrieved successfully',
        data: mockForecastResponse
      };

      service.getForecast('Toronto').subscribe(data => {
        expect(data).toEqual(mockForecastResponse);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/forecast/Toronto`);
      expect(req.request.method).toBe('GET');
      req.flush(apiResponse);
    });

    it('should propagate HTTP errors', () => {
      let errorReceived = false;

      service.getForecast('Unknown').subscribe({
        error: () => { errorReceived = true; }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/forecast/Unknown`);
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });

      expect(errorReceived).toBeTrue();
    });
  });

  describe('searchCities', () => {
    it('should call the correct URL and return search results', () => {
      const apiResponse: ApiResponse<LocationSearchResult[]> = {
        success: true,
        message: 'City search results',
        data: mockSearchResults
      };

      service.searchCities('Tor').subscribe(data => {
        expect(data).toEqual(mockSearchResults);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/search/Tor`);
      expect(req.request.method).toBe('GET');
      req.flush(apiResponse);
    });

    it('should URL-encode search query', () => {
      const apiResponse: ApiResponse<LocationSearchResult[]> = {
        success: true,
        message: 'City search results',
        data: []
      };

      service.searchCities('New Y').subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/search/New%20Y`);
      expect(req.request.url).toContain('New%20Y');
      req.flush(apiResponse);
    });

    it('should propagate HTTP errors', () => {
      let errorReceived = false;

      service.searchCities('x').subscribe({
        error: () => { errorReceived = true; }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/weather/search/x`);
      req.flush('Bad Request', { status: 400, statusText: 'Bad Request' });

      expect(errorReceived).toBeTrue();
    });
  });
});
