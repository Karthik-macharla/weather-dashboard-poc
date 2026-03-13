import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { WeatherComponent } from './weather.component';
import { WeatherCardComponent } from '../weather-card/weather-card.component';
import { WeatherService } from '../../services/weather.service';
import { ForecastResponse, WeatherResponse } from '../../models/weather.model';
import { CommonModule } from '@angular/common';

const mockWeather: WeatherResponse = {
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

const mockForecast: ForecastResponse = {
  location: mockWeather.location,
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

describe('WeatherComponent', () => {
  let component: WeatherComponent;
  let fixture: ComponentFixture<WeatherComponent>;
  let weatherServiceSpy: jasmine.SpyObj<WeatherService>;

  beforeEach(async () => {
    weatherServiceSpy = jasmine.createSpyObj('WeatherService', [
      'getCurrentWeather',
      'getForecast',
      'searchCities'
    ]);

    await TestBed.configureTestingModule({
      declarations: [WeatherComponent, WeatherCardComponent],
      imports: [CommonModule, ReactiveFormsModule],
      providers: [{ provide: WeatherService, useValue: weatherServiceSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize searchForm with empty cityName', () => {
    expect(component.searchForm).toBeTruthy();
    expect(component.cityNameControl?.value).toBe('');
  });

  it('should start with no weather data, no error, and not loading', () => {
    expect(component.weather).toBeNull();
    expect(component.forecast).toBeNull();
    expect(component.isLoading).toBeFalse();
    expect(component.errorMessage).toBeNull();
  });

  describe('form validation', () => {
    it('should mark form as invalid when city name is empty', () => {
      component.searchForm.patchValue({ cityName: '' });
      expect(component.searchForm.invalid).toBeTrue();
    });

    it('should mark form as valid when city name is provided', () => {
      component.searchForm.patchValue({ cityName: 'Toronto' });
      expect(component.searchForm.valid).toBeTrue();
    });

    it('should mark form as invalid when city name exceeds 50 characters', () => {
      component.searchForm.patchValue({ cityName: 'A'.repeat(51) });
      expect(component.searchForm.invalid).toBeTrue();
    });
  });

  describe('onSearch', () => {
    it('should not call service when form is invalid', () => {
      component.searchForm.patchValue({ cityName: '' });
      component.onSearch();
      expect(weatherServiceSpy.getCurrentWeather).not.toHaveBeenCalled();
    });

    it('should call getCurrentWeather when form is valid', () => {
      weatherServiceSpy.getCurrentWeather.and.returnValue(of(mockWeather));
      weatherServiceSpy.getForecast.and.returnValue(of(mockForecast));

      component.searchForm.patchValue({ cityName: 'Toronto' });
      component.onSearch();

      expect(weatherServiceSpy.getCurrentWeather).toHaveBeenCalledWith('Toronto');
    });

    it('should set weather data on successful fetch', () => {
      weatherServiceSpy.getCurrentWeather.and.returnValue(of(mockWeather));
      weatherServiceSpy.getForecast.and.returnValue(of(mockForecast));

      component.searchForm.patchValue({ cityName: 'Toronto' });
      component.onSearch();

      expect(component.weather).toEqual(mockWeather);
      expect(component.forecast).toEqual(mockForecast);
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBeNull();
    });

    it('should set errorMessage on getCurrentWeather failure', () => {
      weatherServiceSpy.getCurrentWeather.and.returnValue(throwError(() => new Error('Not found')));

      component.searchForm.patchValue({ cityName: 'UnknownCity' });
      component.onSearch();

      expect(component.errorMessage).toBe(
        'City not found. Please check the spelling and try again.'
      );
      expect(component.isLoading).toBeFalse();
    });

    it('should set errorMessage on getForecast failure', () => {
      weatherServiceSpy.getCurrentWeather.and.returnValue(of(mockWeather));
      weatherServiceSpy.getForecast.and.returnValue(throwError(() => new Error('Server error')));

      component.searchForm.patchValue({ cityName: 'Toronto' });
      component.onSearch();

      expect(component.errorMessage).toBe(
        'Unable to load forecast data. Please try again.'
      );
      expect(component.isLoading).toBeFalse();
    });

    it('should clear previous data before new search', () => {
      component['weather'] = mockWeather;
      component['forecast'] = mockForecast;

      weatherServiceSpy.getCurrentWeather.and.returnValue(of(mockWeather));
      weatherServiceSpy.getForecast.and.returnValue(of(mockForecast));

      component.searchForm.patchValue({ cityName: 'Toronto' });
      component.onSearch();

      expect(component.weather).toEqual(mockWeather);
    });

    it('should trim whitespace from city name', () => {
      weatherServiceSpy.getCurrentWeather.and.returnValue(of(mockWeather));
      weatherServiceSpy.getForecast.and.returnValue(of(mockForecast));

      component.searchForm.patchValue({ cityName: '  Toronto  ' });
      component.onSearch();

      expect(weatherServiceSpy.getCurrentWeather).toHaveBeenCalledWith('Toronto');
    });
  });

  describe('template rendering', () => {
    it('should show error message when errorMessage is set', () => {
      component.errorMessage = 'Something went wrong';
      fixture.detectChanges();

      const el: HTMLElement = fixture.nativeElement;
      expect(el.querySelector('.weather-dashboard__error')?.textContent).toContain('Something went wrong');
    });

    it('should show loading indicator when isLoading is true', () => {
      component.isLoading = true;
      fixture.detectChanges();

      const el: HTMLElement = fixture.nativeElement;
      expect(el.querySelector('.weather-dashboard__loading')).toBeTruthy();
    });

    it('should show weather card when weather data is available', () => {
      weatherServiceSpy.getCurrentWeather.and.returnValue(of(mockWeather));
      weatherServiceSpy.getForecast.and.returnValue(of(mockForecast));

      component.searchForm.patchValue({ cityName: 'Toronto' });
      component.onSearch();
      fixture.detectChanges();

      const el: HTMLElement = fixture.nativeElement;
      expect(el.querySelector('app-weather-card')).toBeTruthy();
    });

    it('should display validation error when empty city is submitted', () => {
      component.searchForm.patchValue({ cityName: '' });
      component.onSearch();
      fixture.detectChanges();

      const el: HTMLElement = fixture.nativeElement;
      expect(el.querySelector('.search-group__error')).toBeTruthy();
    });
  });

  describe('lifecycle', () => {
    it('should complete destroy$ on ngOnDestroy', () => {
      const completeSpy = spyOn(component['destroy$'], 'complete').and.callThrough();
      component.ngOnDestroy();
      expect(completeSpy).toHaveBeenCalled();
    });
  });
});
